import zbar
from PIL import Image
import cv2
import numpy as np
import io
import struct

# From https://goo.gl/CjhQpC
def unit_vector(vector):
    """ Returns the unit vector of the vector.  """
    return vector / np.linalg.norm(vector)

# From https://goo.gl/CjhQpC
def angle_between(v1, v2):
    """ Returns the angle in radians between vectors 'v1' and 'v2'::

            >>> angle_between((1, 0, 0), (0, 1, 0))
            1.5707963267948966
            >>> angle_between((1, 0, 0), (1, 0, 0))
            0.0
            >>> angle_between((1, 0, 0), (-1, 0, 0))
            3.141592653589793
    """
    v1_u = unit_vector(v1)
    v2_u = unit_vector(v2)
    return np.arccos(np.clip(np.dot(v1_u, v2_u), -1.0, 1.0))

# From a four-array of QR code point positions, get the state_estimate
def get_state_estimate_from_point_locations(locations):

    # Calibrated edge length is used to estimate distance
    camera_fov = np.deg2rad(50.0)
    calibrated_edge_length_pixels = 285.00
    calibrated_edge_length_real = 0.6
    calibrated_distance = 2.0
    expected_center = np.array([512, 512])
    near_distance = 0.3
    calibrated_frustum_height = 2.0 * calibrated_distance * np.tan(camera_fov * 0.5)
    # calibrated_percentage_of_frustum_height = calibrated_edge_length_pixels / calibrated_frustum_height


    a = np.array(locations[0])
    b = np.array(locations[1])
    c = np.array(locations[2])
    d = np.array(locations[3])


    # focal_length = 1024.0 / np.tan(camera_fov / 2.0)

    # print 'Focal length: ' + str(focal_length)

    average_edge_length = np.average([np.linalg.norm(a-b), np.linalg.norm(b-c), np.linalg.norm(c-d), np.linalg.norm(d-a)])

    # frustum_height_fraction = average_edge_length / 1024.0

    # estimated_distance = 0.5 * calibrated_edge_length_real * focal_length / average_edge_length - near_distance

    # estimated_distance = average_edge_length * 0.5 / np.tan(camera_fov / 2.0)

    estimated_frustum_height  = average_edge_length / (calibrated_edge_length_pixels / calibrated_frustum_height)

    estimated_distance = 8.0 / (estimated_frustum_height / np.tan(camera_fov * 0.5))

    # estimated_distance =  calibrated_distance / average_edge_length * (calibrated_edge_length_real / calibrated_edge_length_pixels)


    # scale = calibrated_edge_length_real / calibrated_edge_length_pixels
    # world_distance = estimated_distance

    average_rotation_angle = np.average([angle_between(b-a, (0, 1)), angle_between(c-b, (1, 0)), angle_between(c-d, (0, 1)), angle_between(d-a, (1, 0))])

    print 'Rotation angle: ' + str(np.rad2deg(average_rotation_angle))

    print 'Edge length: ' + str(average_edge_length)

    print 'Calculated distance: ' + str(estimated_distance)

    # Get center and use to calculate relative position
    center = np.average((a, b, c, d), 0)
    relative_position_xy_pixel_space = expected_center - center

    relative_position_xy = relative_position_xy_pixel_space * (calibrated_edge_length_real / average_edge_length)

    # Rotate y and z axes
    relative_position = (estimated_distance, relative_position_xy[1], -relative_position_xy[0])

    print 'center: ' + str(center)
    print 'relative_position: ' + str(relative_position)
    
    packed_state = pack_state(np.rad2deg(average_rotation_angle), relative_position)

    return packed_state

def pack_state(x_rot, pos):
    values = (x_rot, 0.0, 0.0, pos[0], pos[1], pos[2])
    s = struct.Struct('< 6f')
    packed_data = s.pack(*values)
    return packed_data

def image_data_to_state_estimate(raw_bytes, request_id):
    scanner = zbar.ImageScanner()
    # # scanner.parse_config('enable')

    img = Image.open(io.BytesIO(raw_bytes))

    # Uncomment for testing
    # img = Image.open('qr_code.png')

    # Save a copy of image
    file = open('downlinked_images/' + str(request_id).zfill(10) + '.jpg', 'wb')
    file.write(raw_bytes)

    frame = img.convert("RGB")
    gray = cv2.cvtColor(np.array(frame), cv2.COLOR_BGR2GRAY, dstCn=0)
    pil_img = Image.fromarray(gray)
    width, height = pil_img.size
    pil_bytes = pil_img.tobytes()

    # Get file content
    # raw_bytes = open('qr_code.png', 'rb').read()

    # create a reader
    # width = height = 1024
    zImg = zbar.Image(width, height, 'Y800', pil_bytes)
    
    scanner.scan(zImg)

    # extract results


    if len(zImg.symbols) > 0:
        # do something useful with results
        symbol = list(zImg.symbols)[0]
        print('Decoded symbol with data: %s' % symbol.data)
        print(symbol.type)
        print(symbol.location)
        
        return get_state_estimate_from_point_locations(symbol.location)

    else:
        print('No QR codes found!')
        return ''


