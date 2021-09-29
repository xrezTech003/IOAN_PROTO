<?php
    //Example
    //http://localhost/ioan_proximity_query.php?xPos=81&zPos=-70&radius=0.01&paramEnum=1&paramVal=10
    
    //Init
    $servername = "localhost";
    $username   = "ioan";
    $password   = "ioan2017";
    $dbname     = "mydb";

    //Get Pseudo Ra and Dec of player position
    $ra  = $_GET['xPos'];
    $dec = $_GET['zPos'];
    $rad = $_GET['radius'];
    $par = $_GET['paramEnum'];
    $val = $_GET['paramVal'];

    //Connect to DB
    $conn = new mysqli($servername, $username, $password, $dbname);
    if ($conn->connect_error) die("Connection Failed: " . $conn->connect_error);
    //else echo "CONNECTED";
?>

<?php
    //Parameter Switching
    $parameters = array(0 => "RMS",
                        1 => "SNR",
                        2 => "astrometric_pseudo_colour",
                        3 => "variable_flag");
    $param = $parameters[$par];

    //Query DB
    $query = "SELECT `ioan_id` FROM ".
             "(Select * FROM `ioan_overview` ".
             "WHERE SQRT(POW(`ra` - $ra, 2) + POW(`dec` - $dec, 2)) < $rad LIMIT 10) ".
             "AS NearbyStars ".
             "ORDER BY ABS(IFNULL($param, 1000) - $val) LIMIT 1";
    $result = $conn->query($query);
    if (mysqli_error($conn)) echo mysqli_error($conn);
    
    //Parse Results
    if ($result->num_rows > 0)
    {
      while($row = $result->fetch_assoc())
      {
          echo $row["ioan_id"] . "\r\n";
      }
    }
    else echo "0 results";
?>

<?php
    //End Connection
    $conn->close();
?>