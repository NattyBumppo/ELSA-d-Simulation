from flask import Flask, request, make_response
import decode_qrcode

global request_count

request_count = 0

def image_bytes_to_state_estimate(bytes, request_id):

    return decode_qrcode.image_data_to_state_estimate(bytes, request_id)
    
def main():

    app = Flask(__name__)

    @app.route('/new_chaser_image', methods = ['PUT'])
    def new_chaser_image():
        global request_count
        # if request.method == 'PUT':
            # print "new request from %s" % request.remote_addr

        if request.data:
            print "Received data of length %s" % len(request.data)

            bytes = image_bytes_to_state_estimate(request.data, request_count)
            request_count += 1

            response = make_response(bytes)
            return response
        else:
            return "ERROR"

        

    app.run(host='0.0.0.0')


if __name__ == '__main__':
    main()