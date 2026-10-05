import geopandas as gpd
from shapely.geometry import Point
from confluent_kafka import Consumer
import json
import pika
import redis

regions_path = "regions.geojson"

def get_region_with_geopandas(file_path: str, lon: float, lat: float) -> str:
    gdf = gpd.read_file(file_path)
    pt = Point(lon, lat)
    matched = gdf[gdf.geometry.contains(pt)]
    if not matched.empty:
        return matched.iloc[0]["region"]
    return "OVERSEAS"

redis_client = redis.Redis(host="localhost", port=6379, decode_responses=True)

connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))

channel = connection.channel()

channel.queue_declare(queue="NORTH", durable=True)
channel.queue_declare(queue="CENTER", durable=True)
channel.queue_declare(queue="SOUTH", durable=True)
channel.queue_declare(queue="OVERSEAS", durable=True)

consumer = Consumer({
        "bootstrap.servers": "localhost:9092",
        "group.id": "my-consumer",
        "auto.offset.reset": "earliest"})
consumer.subscribe(["alerts"])

def is_alert_data_valid(data) -> str:
    result = ""
    if data["priority"] not in ["CRITICAL", "HIGH", "MEDIUM", "LOW"]:
        result += "invalid priority"
    if data["classification"] not in ["UNCLASSIFIED", "RESTRICTED", "SECRET", "TOP_SECRET"]:
        result += "invalid classification"
    if -180 > data["lon"] or data["lon"] > 180:
        result += "invalid lon"
    if -90 > data["lat"] or data["lat"] > 90:
        result += "invalid lat"
    if data["status"] != "WAITING":
        result += "invalid status"
    if data["source"] not in ["shabak", "pikud-haoref", "mossad", "aman"]:
        result += "invalid source"
    return result

while True:
    msg = consumer.poll(10)
    if msg is None:
        continue

    data = json.loads(msg.value().decode("utf-8"))

    if is_alert_data_valid(data) != "":
        continue # + dont forget log!
    
    if redis_client.exists(f"{data}"):
        consumer.commit(msg)
        continue #log!

    redis_client.set(f"{data}", ex=10)

    region = get_region_with_geopandas(regions_path, float(data["lon"]), float(data["lat"]))
    channel.basic_publish(exchange="", routing_key= f"{region}", body=data)
    consumer.commit(msg)





