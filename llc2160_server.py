from flask import Flask, Response, request
import msgpack
import numpy as np
import openvisuspy as ovp
import os
os.environ['VISUS_CACHE']= "./visus_can_be_deleted"

# LLC2160 data
u_url = "https://maritime.sealstorage.io/api/v0/s3/utah/nasa/dyamond/mit_output/llc2160_arco/visus.idx?access_key=any&secret_key=any&endpoint_url=https://maritime.sealstorage.io/api/v0/s3&cached=arco"
w_url = "https://maritime.sealstorage.io/api/v0/s3/utah/nasa/dyamond/mit_output/llc2160_w/llc2160_w.idx?access_key=any&secret_key=any&endpoint_url=https://maritime.sealstorage.io/api/v0/s3&cached=arco"
v_url = "https://maritime.sealstorage.io/api/v0/s3/utah/nasa/dyamond/mit_output/llc2160_v/v_llc2160_x_y_depth.idx?access_key=any&secret_key=any&endpoint_url=https://maritime.sealstorage.io/api/v0/s3&cached=arco"

u_db=ovp.LoadDataset(u_url)
w_db=ovp.LoadDataset(w_url)
v_db=ovp.LoadDataset(v_url)

app = Flask(__name__)

@app.route("/", methods=["GET"])
def serve_array():
    try:
        quality = int(request.args.get("quality", 0))
        time = int(request.args.get("time", 0))
        z = [int(x) for x in request.args.getlist("z")]
        x_range = [int(x) for x in request.args.getlist("x_range")]
        y_range = [int(x) for x in request.args.getlist("y_range")]
    except Exception as e:
        return jsonify({"error": f"Invalid input parameters {str(e)}"}), 400
    print(f"quality: {quality} time: {time} z: {z} x_range: {x_range} y_range: {y_range}")
    u_array = u_db.db.read(time=time, z=z, x=x_range, y=y_range, quality=quality)
    w_array = w_db.db.read(time=time, z=z, x=x_range, y=y_range, quality=quality)
    v_array = v_db.db.read(time=time, z=z, x=x_range, y=y_range, quality=quality)

    payload = {
        "shape": u_array.shape,
        "dtype": "float32",
        "u_array": u_array.tobytes(),
        "w_array": w_array.tobytes(),
        "v_array": v_array.tobytes()
    }

    packed = msgpack.packb(payload, use_bin_type=True)
    return Response(packed, content_type="application/x-msgpack")

if __name__ == "__main__":
    app.run(port=5000)