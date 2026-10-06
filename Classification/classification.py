import geopandas as gpd
from shapely.geometry import Point
from confluent_kafka import Consumer
from elasticsearch import Elasticsearch
import datetime
import json
import pika
import redis
import random

regions_path = "regions.geojson"
local_host = "localhost"
kafka_connection = "localhost:9092"
kafka_topic = "alerts"
redis_key = "alerts"
exchange = "alerts"
elastic_uri = "http://localhost:9200"

validPriority = ["CRITICAL", "HIGH", "MEDIUM", "LOW"]
validClassification = ["UNCLASSIFIED", "RESTRICTED", "SECRET", "TOP_SECRET"]
validSource = ["shabak", "pikud-haoref", "mossad", "aman"]



def get_region_with_geopandas(file_path: str, lon: float, lat: float) -> str:
    gdf = gpd.read_file(file_path)
    pt = Point(lon, lat)
    matched = gdf[gdf.geometry.contains(pt)]
    if not matched.empty:
        return matched.iloc[0]["region"]
    return "OVERSEAS"

redis_client = redis.Redis(host=local_host, port=6379, decode_responses=True)
connection = pika.BlockingConnection(pika.ConnectionParameters(local_host))
channel = connection.channel()

channel.queue_declare(queue="NORTH", durable=True)
channel.queue_declare(queue="CENTER", durable=True)
channel.queue_declare(queue="SOUTH", durable=True)
channel.queue_declare(queue="OVERSEAS", durable=True)

consumer = Consumer({
        "bootstrap.servers": kafka_connection,
        "group.id": "new" + str(random.random()),
        "auto.offset.reset": "earliest"})
consumer.subscribe([kafka_topic])

def is_alert_data_valid(data) -> str:
    result = ""
    if data["priority"] not in validPriority:
        result += "invalid priority"
    if data["classification"] not in validClassification:
        result += "invalid classification"
    if -180 > data["lon"] or data["lon"] > 180:
        result += "invalid lon"
    if -90 > data["lat"] or data["lat"] > 90:
        result += "invalid lat"
    if data["status"] != "WAITING":
        result += "invalid status"
    if data["source"] not in validSource:
        result += "invalid source"
    return result

es = Elasticsearch("http://localhost:9200")

while True:
    msg = consumer.poll()
    if msg is None:
        break

    data = json.loads(msg.value().decode("utf-8"))
    print(data)
    
    region = get_region_with_geopandas(regions_path, float(data["lon"]), float(data["lat"]))
    data["region"] = region
    
    if is_alert_data_valid(data) != "":
        print("skipped alert")
        es.index(index= "logs", id=1, document={
            "Level": "Warning",
            "Source": "Classification",
            "Content": f"alert_id: {data["alert_id"]} is not valid",
            "Timestamp": datetime.datetime.now()})
        continue

    if redis_client.exists(f"{data["alert_id"]}"):
        consumer.commit(msg)
        print("already in redis")
        es.index(index="logs",id=1,document={
            "Level": "Warning",
            "Source": "Classification",
            "Content": f"alert_id: {data["alert_id"]} already sent",
            "Timestamp": datetime.datetime.now()})
        continue 

    redis_client.set(name= redis_key, value= json.dumps(data["alert_id"]), ex= 20)
    print("set into redis")

    channel.basic_publish(exchange="", routing_key= f"{region}", body=json.dumps(data))
    print("sent into proper queue")
    consumer.commit(msg)