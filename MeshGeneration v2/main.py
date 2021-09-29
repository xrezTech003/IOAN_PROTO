import os
import MySQLdb as my
import numpy as np
import struct
import time
from scipy.spatial import Delaunay
from plot_parallax import PlotParallax


# These are the "bad limits" used to normalize the data
# These limits are not the true max and min values but yield a higher gradient of normalized values
def bad_limits():
    return [
        [85.3308837157859, 77.928459],  # ra
        [-68.2219663818328, -72.003059],  # dec
        [19.061404043487336, 14.886403072230165],  # mag
        [0.6680229918890426, 0],  # std
        [3125.7502367831025, 0, 922.3292],  # obs
        [3, 1],  # var_flag
        [7, 0],  # catalog
        [1.8991459147993115, 1.231199705120561],  # pseudo_color
        [36.09744, 0.263666],  # period
        [26.61842977741538, 0],  # SNR
        [0.6715103569670999, 0],  # RMS
        [11.408797042686029, 0, 3.138575],  # pmra
        [4.204787, -6.172524]  # parallax
    ]


# These limits are the true max and min values gathered from the database
def good_limits():
    return [
        [85.3308837157859, 77.928459],  # ra
        [-68.2219663818328, -72.003059],  # dec
        [20.7929359999999, 6.91054206349206],  # mag
        [3.72457197182105, 0.00701685703283053],  # std
        [6694, 1, 922.32918319016],  # obs
        [3, 1],  # var_flag
        [6, 4],  # catalog
        [3.46534939690518, -0.152678124790899],  # pseudo_color
        [8065, 0],  # period
        [123.266985003101, 0.664492034337461],  # SNR
        [3.72457197182105, 0],  # RMS
        [832.604339112427, -259.942043713013, 1.91200390316532],  # pmra
        [117.309111122772, -162.373621536621]  # parallax
    ]


# Take in the limits and output them in byte format to "limits.b"
def output_byte_header(limits):
    with open("mesh_data/limits.b", "wb+") as file:
        for i in range(13):
            for j in range(len(limits[i])):
                file.write(bytearray(struct.pack("d", limits[i][j])))
    return


# Given coordinate and data, output star data as bytes to their respective .b file
def output_byte_data(x, y, results):
    points = []  # Array for (x, z) coordinates
    for row in results:
        points.append([row[1], row[2]])
    points = np.array(points)  # Convert to numpy array
    tri = Delaunay(points)  # Perform Delaunay Triangulation

    with open("mesh_data/grid" + str(x) + "-" + str(y) + ".b", "wb+") as file:  # Output star data
        # Output header data
        file.write(bytearray(struct.pack("I", len(results))))
        file.write(bytearray(struct.pack("H", 80)))
        file.write(bytearray(struct.pack("I", len(tri.simplices))))
        file.write(bytearray(struct.pack("H", 12)))
        file.write(bytearray(struct.pack("I", 0)))

        for row in results:  # Output each set of star data
            file.write(bytearray(struct.pack("I", int(row[0]))))  # star_id / uint
            file.write(bytearray(struct.pack("f", float(row[1]))))  # ra / float
            file.write(bytearray(struct.pack("f", float(row[2]))))  # dec / float
            file.write(bytearray(struct.pack("d", float(row[3]))))  # mean_mag / double
            file.write(bytearray(struct.pack("d", float(row[4]))))  # mag_std / double
            file.write(bytearray(struct.pack("H", int(row[5]))))  # num_obs / double
            file.write(bytearray(struct.pack("H", int(row[6]))))  # variability / ushort
            file.write(bytearray(struct.pack("H", int(row[0] / 10000000))))  # catalog / ushort
            file.write(bytearray(struct.pack("d", float(row[7]))))  # astrocolor / double
            file.write(bytearray(struct.pack("d", float(row[8]))))  # period / double
            file.write(bytearray(struct.pack("d", float(row[9]))))  # snr / double
            file.write(bytearray(struct.pack("d", float(row[10]))))  # rms / double
            file.write(bytearray(struct.pack("H", 0 if row[11] is None else 1)))  # var_class / ushort
            file.write(bytearray(struct.pack("d", float(row[12]))))  # pmra / double
            file.write(bytearray(struct.pack("f", float(row[13]))))  # parallax / float

        for i in tri.simplices:  # Output triangles
            for j in range(3):
                file.write(bytearray(struct.pack("I", i[j])))
    return


# OLD_METHOD; output data to .obj file
def output_obj(x, y, results):
    vs = []
    vcs = []
    vts = []
    vt1s = []

    for row in results:
        vs.append([normalize(row[1], limits[0][1], limits[0][0]),
                   normalize(row[13], limits[12][1], limits[12][0]),
                   normalize(row[2], limits[1][1], limits[1][0])])
        vcs.append([normalize(row[3], limits[2][1], limits[2][0]),
                    normalize(row[4], limits[3][1], limits[3][0]),
                    normalize(row[5], limits[4][1], limits[4][0]),
                    normalize(row[6], limits[5][1], limits[5][0])])
        vts.append([normalize(int(str(row[0])[0]), limits[6][1], limits[6][0]),
                    normalize(row[7], limits[7][1], limits[7][0]),
                    normalize(row[8], limits[8][1], limits[8][0]),
                    normalize(row[9], limits[9][1], limits[9][0])])
        vt1s.append([normalize(row[10], limits[10][1], limits[10][0]),
                     0.0 if row[11] is None else 1.0,  # FIXME?
                     normalize(row[12], limits[11][1], limits[11][0]),
                     row[0]])

    for i in range(len(vs)):
        vs[i][0] = 400.0 * (vs[i][0] - 0.5)
        vs[i][1] = 000.3 * (vs[i][1] - 0.5)
        vs[i][2] = 200.0 * (vs[i][2] - 0.5)

    obj_file = open("bakedMeshes/meshBake" + str(x) + "-" + str(y) + ".obj", "w+")
    obj_file.write("g meshPart 1(Clone)\n")

    for i in vs:
        obj_file.write("v " + str(i[0]) + " " + str(i[1]) + " " + str(i[2]) + "\n")
    obj_file.write("\n")

    for i in vcs:
        obj_file.write("vc " + str(i[0]) + " " + str(i[1]) + " " + str(i[2]) + " " + str(i[3]) + "\n")
    obj_file.write("\n")

    for i in vts:
        obj_file.write("vt " + str(i[0]) + " " + str(i[1]) + " " + str(i[2]) + " " + str(i[3]) + "\n")
    obj_file.write("\n")

    for i in vt1s:
        obj_file.write("vt1 " + str(i[0]) + " " + str(i[1]) + " " + str(i[2]) + " " + str(i[3]) + "\n")
    obj_file.write("\n")

    obj_file.write("usemtl New Material 9\n")
    obj_file.write("usemtl New Material 9\n")

    points = []
    for i in vs:
        points.append([i[0], i[2]])
    points = np.array(points)
    tri = Delaunay(points)

    for i in tri.simplices:
        triangles = []
        for j in range(3):
            triangles.append(str(i[j] + 1) + "/" + str(i[j] + 1) + "/" + str(i[j] + 1))
        obj_file.write("f " + triangles[0] + " " + triangles[1] + " " + triangles[2] + "\n")

    obj_file.close()


# normalize to max and min; NOT USED IN NEW METHOD
def normalize(val, mi, ma):
    return (val - mi) / (ma - mi)


# normalize to max, min, mean, and power; ALSO NOT USED IN NEW METHOD
def normalize_power(val, mi, ma, mean, power):
    mean = normalize(mean, mi, ma)
    val = normalize(val, mi, ma)
    val -= mean

    if val != 0.0:
        val = (pow(abs(val), power) * (abs(val) / val)) + mean

    return val


# connect to the database and query for the given mesh coordinates
def pull_data(x, y, limits, db):
    if os.path.isdir("bakedMeshes") is False:  # Create baked meshes folder
        os.mkdir("bakedMeshes")

    where = """"""  # WHERE clause in SQL query
    # Add all IDs from requested file to WHERE clause
    with open("meshIndices/index" + str(x) + "-" + str(y) + ".txt", "r") as file:
        where += """`ioan_id` IN ("""
        lines = file.readlines()
        first = True
        for line in lines:
            if first:
                first = False
            else:
                where += ""","""
            where += """ """ + line
        where += """)"""

    cursor = db.cursor()  # SQL Query, uses field min if value is null
    cursor.execute("""SELECT `ioan_id`, 
                             `ra`,   
                             `dec`,                              
                              IFNULL(`magnitude`, """ + str(limits[2][1]) + """), 
                              IFNULL(`std`, """ + str(limits[3][1]) + """),   
                              IFNULL(`obs`, """ + str(limits[4][1]) + """),  
                             `variable_flag`,                   
                              IFNULL(`astrometric_pseudo_colour`, """ + str(limits[7][1]) + """),
                              IFNULL(`object_period`, """ + str(limits[8][1]) + """),        
                              IFNULL(`SNR`, """ + str(limits[9][1]) + """),  
                              IFNULL(`RMS`, """ + str(limits[10][1]) + """),                 
                              `variable_type`,                  
                              IFNULL(`pmra`, """ + str(limits[11][1]) + """), 
                              IFNULL(`object_parallax`, """ + str((limits[12][0] + limits[12][1]) / 2.0) + """)
                      FROM `ioan_overview` WHERE """ + where)

    output_byte_data(x, y, cursor.fetchall())


start_time = time.time()  # start timer

limits = bad_limits()  # set limits
output_byte_header(limits)  # output limits

db = my.connect(  # connect to database
        host="127.0.0.1",
        user="root",
        passwd="",
        db="mydb"
    )

# generate all 55 mesh files
for i in range(11):
    for j in range(5):
        pull_data(i + 1, j + 1, limits, db)
        print("(" + str(i + 1) + ", " + str(j + 1) + ") : Complete")

db.close()  # close database connection

duration = time.time() - start_time  # calculate duration
print("LOAD TIME: " + str(duration) + "s")  # output seconds taken to generate
