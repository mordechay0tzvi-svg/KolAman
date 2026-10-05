import geopandas as gpd
from shapely.geometry import Point
from confluent_kafka import Consumer
import json
import pika
import redis
import random

regions_path = "regions.geojson"
localHost = "localhost"
kafkaConnection = "localhost:9092"
kafkaTopic = "alerts"
redisKey = "alerts"


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

redis_client = redis.Redis(host=localHost, port=6379, decode_responses=True)
connection = pika.BlockingConnection(pika.ConnectionParameters(localHost))
channel = connection.channel()

channel.queue_declare(queue="NORTH", durable=True)
channel.queue_declare(queue="CENTER", durable=True)
channel.queue_declare(queue="SOUTH", durable=True)
channel.queue_declare(queue="OVERSEAS", durable=True)

consumer = Consumer({
        "bootstrap.servers": kafkaConnection,
        "group.id": "new" + str(random.random()),
        "auto.offset.reset": "earliest"})
consumer.subscribe([kafkaTopic])

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

while True:
    msg = consumer.poll(10)
    if msg is None:
        break

    data = json.loads(msg.value().decode("utf-8"))
    print(data)
    
    region = get_region_with_geopandas(regions_path, float(data["lon"]), float(data["lat"]))
    data["region"] = region
    
    if is_alert_data_valid(data) != "":
        print("skipped alert")
        continue # + dont forget log!

    if redis_client.exists(f"{data}"):
        consumer.commit(msg)
        print("already in redis")
        continue #log!

    redis_client.set(name= redisKey, value= json.dumps(data), ex= 10)
    print("set into redis")

    channel.basic_publish(exchange="", routing_key= f"{region}", body=json.dumps(data))
    print("sent into proper queue")
    consumer.commit(msg)