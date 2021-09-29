<?php
//New id_loader for the ioan_overview
 $db = mysqli_connect('127.0.0.1:3306','ioan','ioan2017','mydb')
 or die('Error connecting to MySQL server.');
?>

<?php
$start = $_GET['start'];
$cap = $_GET['cap'];
$query = "SELECT ioan_id, DB_Source, magnitude, std,astrometric_pseudo_colour, variable_flag, object_period, SNR, RMS, variable_type, obs, pm_dist, object_parallax FROM ioan_overview LIMIT ". $start .','. $cap;

mysqli_query($db, $query) or die('Error querying database.');
$result = mysqli_query($db, $query);
while($row = mysqli_fetch_array($result)){
  $ioan_id = $row['ioan_id'];
  $source = $row['DB_Source'];
  $mag = number_format((float)($row['magnitude']), 5, '.', '');
  $std = number_format((float)($row['std']), 5, '.', '');
  $p_color = number_format((float)$row['astrometric_pseudo_colour'], 5, '.', '');
  $v_flag = $row['variable_flag'];
  $period = number_format((float)$row['object_period'], 5, '.', '');
  $SNR = number_format((float)$row['SNR'], 5, '.', '');
  $RMS = number_format((float)$row['RMS'], 5, '.', '');
  $v_type = $row['variable_type'];
  $obs = $row['obs'];
  $pm_dist = number_format((float)$row['pm_dist'], 5, '.', '');
  $parallax = number_format((float)$row['object_parallax'], 5, '.', '');
  echo $ioan_id, ',', empty($mag)?0:$mag, ',',empty($std)?0:$std, ',',
empty($obs)?0:$obs, ',',
  $v_flag, ',', $source, ',' , empty($p_color)?0:$p_color, ',',
empty($period)?0:$period, ',',
  empty($SNR)?0:$SNR, ',',empty($RMS)?0:$RMS, ',',empty($v_type)?'unknown':$v_type, ',',
  




empty($pm_dist)?0:$pm_dist, ',',
 empty($parallax)?0:$parallax,
' ';
  
  
}

  mysqli_close($db);

?>
