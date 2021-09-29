<?php
//
//Sam Johnson 3/23/2017
//  This code will call the 3 IOAN database and return info for
//  the argument passed as id.
//
//  Example:
//    localhost/newidtester.php?id=40000001
//
 $db = mysqli_connect('127.0.0.1:3306','ioan','ioan2017')
 or die('Error connecting to MySQL server.');
?>
<?php
$id = $_GET['id'];
echo "--Ast3_information-- ";
$query = "SELECT * FROM mydb.ioan_all WHERE ioan_id =" . $id;
mysqli_query($db, $query) or die('Error querying database.');
$result = mysqli_query($db, $query);
if($row = mysqli_fetch_assoc($result)){
  echo "<br/>fields ", sizeof($row), " ";
  echo "<br/>result positive ";
  foreach($row as $cname => $cvalue){
	  if(!is_numeric($cvalue) && empty($cvalue)){
		  print "<br/>$cname no_result ";
	  }
	  else{
		  echo "<br/>", $cname, " ", str_replace(' ', '_', $cvalue), " ";  
	  }
  }
}
else{
	$query = "SHOW COLUMNS FROM mydb.ioan_all";
	mysqli_query($db, $query) or die('Error querying database.');
	$result = mysqli_query($db, $query);
	$num = mysqli_num_rows($result);
	echo "<br/>fields ", $num, " ";
	echo "<br/>result negative ";
	while($row = mysqli_fetch_array($result)){
		echo "<br/>", $row[0], " no_result ";
	}
}
echo "<br/><br/>--simbad_results-- ";
$query = "SELECT * FROM mydb.ioan_simbad WHERE ioan_id = " . $id;
mysqli_query($db, $query) or die('Error querying database.');
$result = mysqli_query($db, $query);
if($row = mysqli_fetch_assoc($result)){
  echo "<br/>fields ", sizeof($row), " ";
  echo "<br/>result positive ";
  foreach($row as $cname => $cvalue){
	  if(!is_numeric($cvalue) && empty($cvalue)){
		  print "<br/>$cname no_result ";
	  }
	  else{
		  echo "<br/>", $cname, " ", str_replace(' ', '_', $cvalue), " ";  
	  }
  }
}
else{
	$query = "SHOW COLUMNS FROM mydb.ioan_simbad";
	mysqli_query($db, $query) or die('Error querying database.');
	$result = mysqli_query($db, $query);
	$num = mysqli_num_rows($result);
	echo "<br/>fields ", $num, " ";
	echo "<br/>result negative ";
	while($row = mysqli_fetch_array($result)){
		echo "<br/>", $row[0], " no_result ";
	}
}
echo "<br/><br/>--gaia_results-- ";
$query = "SELECT * FROM mydb.ioan_gaia WHERE ioan_id = " . $id;
mysqli_query($db, $query) or die('Error querying database.');
$result = mysqli_query($db, $query);
if($row = mysqli_fetch_assoc($result)){
  echo "<br/>fields ", sizeof($row), " ";
  echo "<br/>result positive ";
  foreach($row as $cname => $cvalue){
	  if(!is_numeric($cvalue) && empty($cvalue)){
		  print "<br/>$cname no_result ";
	  }
	  else{
		  echo "<br/>", $cname, " ", str_replace(' ', '_', $cvalue), " ";   
	  }
  }
}
else{
	$query = "SHOW COLUMNS FROM mydb.ioan_gaia";
	mysqli_query($db, $query) or die('Error querying database.');
	$result = mysqli_query($db, $query);
	$num = mysqli_num_rows($result);
	echo "<br/>fields ", $num, " ";
	echo "<br/>result negative ";
	while($row = mysqli_fetch_array($result)){
		echo "<br/>", $row[0], " no_result ";
	}
}
mysqli_close($db);
?>