<?php
//I Chen Yeh 8/02/2018
//New version of individual object query modified for aws server
//Fit the description with figure location.
//Example:
//http://192.168.1.129/ioan_newidtester_fig_2.php?id=40000006
 $db = mysqli_connect('localhost','ioan','ioan2017')
 or die('Error connecting to MySQL server.');
?>

<?php
$id = $_GET['id'];
$localIP = getHostByName(getHostName());
$remoteIP = $_SERVER['REMOTE_ADDR'];

function variable_check($cname, $cvalue){
	if(!is_numeric($cvalue) && empty($cvalue)){
		print "<br/>$cname Not_Available ";
	}
	else{
		  echo "<br/>", $cname, " ", str_replace(' ', '_', $cvalue), " ";
	}
}
?>
<?php
function resize_img($file, $w, $h,$temp_loc ,$crop=True){
	list($ow, $oh) = getimagesize($file);
	$src = imagecreatefrompng($file);
	$dst = imagecreatetruecolor($w, $h);
	$black = imagecolorallocate($dst, 0,0,0);
	imagecopyresampled($dst, $src, 0 ,0, 0, 0, $w, $h, $ow, $oh);
	imagecolortransparent($dst, $black);
	imagepng($dst, $temp_loc, 0);
	return $dst;
}
?>
<?php
	function image_counter($request){
		$local_file = "counter/".$request.".txt";
		$current_hit = 0;
		if (file_exists($local_file)){
			$fp = fopen($local_file, "r+");
			$prev_count = fgets($fp, 1024);
			//fclose($fp);
			$current_hit = $prev_count +1;
		}
		else{
			$fp = fopen($local_file, "w");
		}
		if ($current_hit%10 == 0){
			$current_hit = 0;
		}
		//$fp = fopen($local_file, "w");
		rewind($fp);
		fwrite($fp, $current_hit);
		fclose($fp);
		return $current_hit;
	}

?>
<?php
echo "AST3-LMC ";
$selection = array("ioan_id", "ra", "dec", "obs", "magnitude", "RMS", "SNR", "object_period", "variable_flag", "variable_type");
$mask_name = array("AST3_ID", 'RA', "DEC", "OBS", "i_MAG", "RMS", "LS_SNR", "LS_Period(day)", "Variability", "Type");
$ast3_query = "SELECT `".implode("`,`", $selection)."` from mydb.ioan_overview WHERE ioan_id=".$id;
mysqli_query($db, $ast3_query) or die('Error during ast3 query.');
$result = mysqli_query($db, $ast3_query);
if($row = mysqli_fetch_assoc($result)){
	echo "<br/>fleids ", sizeof($row), " ";
	foreach(array_combine($selection , $mask_name) as $cname => $mask_name){
		$v_flag = 0;
		switch($cname){

			case "variable_flag":
				//$des = "Variability";
				if ($row[$cname]==1){
					$v_flag = 1;
					variable_check($mask_name, "Confirmed");
				}
				else{
					variable_check($mask_name,"Unconfirmed");
				}
				break;
			case "variable_type":
				if(empty($row[$cname]) && $v_flag==1){
					variable_check($mask_name, "Unspecified");
				}
				else{
					variable_check($mask_name, $row[$cname]);
				}
				break;
			default:
				variable_check($mask_name, $row[$cname]);
		}

	}
}
else{
	echo "<br/>fields ", sizeof($mask_name), " ";
	foreach($mask_name as $cname){
		variable_check($cname, "Not_Available");
	}
}
?>

<?php
echo "<br/><br/>GAIA-DR2 ";
$selection = array("source_id", "astrometric_pseudo_colour", "parallax", "teff_val", "bp_rp", "bp_g", "g_rp", "pmra", "pmdec");
$mask_name = array("GAIA_DR2_ID", 'Pseudocolor', "Parallax", "Teff", "BP_RP", "BP_G", "G_RP", "pmra", "pmdec");
$gaia_query = "SELECT `".implode("`,`", $selection)."` from mydb.ioan_gaia WHERE ioan_id=".$id;
mysqli_query($db, $gaia_query) or die('Error during gaia query.');
$result = mysqli_query($db, $gaia_query);
if($row = mysqli_fetch_assoc($result)){
	echo "<br/>fleids ", sizeof($row), " ";
	foreach(array_combine($selection , $mask_name) as $cname => $mask_name){
		variable_check($mask_name, $row[$cname]);
	}
}
else{
	echo "<br/>fields ", sizeof($mask_name), " ";
	foreach($mask_name as $cname){
		variable_check($cname, "Not_Available");
	}
}
?>

<?php
echo "<br/><br/>SIMBAD ";
$selection = array("oid", "main_id", "otype_longname", "sp_type", "rvz_radvel");
$mask_name = array("SIMBAD_OID", "Object_Name", "Object_Type", "Spectral_Type", "Radial_Velocity");
$simbad_query = "SELECT `".implode("`,`", $selection)."` from mydb.ioan_simbad WHERE ioan_id=".$id;
mysqli_query($db, $simbad_query) or die('Error during simbad query.');
$result = mysqli_query($db, $simbad_query);
if($row = mysqli_fetch_assoc($result)){
	echo "<br/>fleids ", sizeof($row), " ";
	foreach(array_combine($selection , $mask_name) as $cname => $mask_name){
		variable_check($mask_name, $row[$cname]);
	}
}
else{
	echo "<br/>fields ", sizeof($mask_name), " ";
	foreach($mask_name as $cname){
		variable_check($cname, "Not_Available");
	}
}
?>
<?php
echo "<br/><br/>Figure ";
$query = "select `fig_loc` from mydb.ioan_fig_all where ioan_id = " . $id;
mysqli_query($db, $query) or die('Error querying database.');
$result = mysqli_query($db, $query);
if ($row = mysqli_fetch_assoc($result)){
	$fig_loc = $row['fig_loc'];
	$url = "http://".$localIP."/fig_cmp/" . $fig_loc;
	#echo "<br/>".$url;
	$local_fp = str_replace(".", "_", $remoteIP);
	$temp_loc = "resize_img/".$local_fp."_".image_counter($local_fp).".png";
	$image=resize_img($url,600,600,$temp_loc);
	echo "<br/>http://".$localIP."/".$temp_loc;
}
else{
	$url = "http://".$localIP."/fig_cmp/empty_fig.png";
	echo "<br/>".$url;
}


mysqli_close($db);

?>
