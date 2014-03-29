<?php

  	$errors = array();
	$showTimezone = false;
	$service = 'api.ipinfodb.com';
	$version = 'v2';
	$apiKey = '';
	
	
	function setKey($key){
		if(!empty($key)) $apiKey = $key;
	}

	function showTimezone(){
		$showTimezone = false;
	}

	function getError(){
		return implode("\n", errors);
	}

	function getGeoLocation($ip)
	{
		//$ip = @gethostbyname($host);

		if(preg_match('/^(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]\d|\d)(?:[.](?:25[0-5]|2[0-4]\d|1\d\d|[1-9]\d|\d)){3}$/', $ip)){
			$xml = file_get_contents('http://' . $service . '/' . $version . '/' . 'ip_query.php?key=' . $apiKey . '&ip=' . $ip);
			echo $xml;
			//try
			{
				$response = @new SimpleXMLElement($xml);

				foreach($response as $field=>$value){
					$result[(string)$field] = (string)$value;
				}
				
				return $result;
			}
			//catch(Exception $e){
				$errors[] = $e->getMessage();
			//	return;
			//}
		}

		$errors[] = '"' . $host . '" is not a valid IP address or hostname.';
		echo '"' . $host . '" is not a valid IP address or hostname.';
		return;
	}
	
	function GetCityFromIP($IP)
	{
		//Load the class
		
		setKey('100198fe7b9541fe7f7ee65a42cbebe6a525a18c728e671ec05cfab2e131189b');
		echo "<strong>Geolocating".$IP."</strong><br />\n";
		 
		//Get errors and locations
		$locations = getGeoLocation($IP);
		$errors = getError();
		 
		//Getting the result
		//echo "<p>\n";
		//echo "<strong>First result</strong><br />\n";
		if (!empty($locations) && is_array($locations)) 
		{
		  foreach ($locations as $field => $val) 
		  {
			$ret = $ret. $field . ' : ' . $val . "<br />\n";
		  }
		}
		return $ret;
 
	}

  if($_SERVER['SERVER_NAME']=="localhost")
    $dbname='../db_stats/base_dwnload.sqlite';
  else
    $dbname='../data/DB/Routeur_Dwnl.sqlite';
 
    $mytable ="dwnload";

  $base= sqlite_open($dbname, 0666, $err);
  if (!$err)
  {
    $result = sqlite_array_query($base, "SELECT DISTINCT file_name FROM $mytable order by file_name",SQLITE_ASSOC,$err_str);
   
    echo "<h1>Stat telechargement par fichier</h1>";
    
    echo  "<center><table border=1 cellspacing='0' cellpadding='1'\n";
    echo "<tr><th>File name</th><th>Nb real download</th><th>Total download</th></tr>";
    
    foreach($result as $entry)
    {
      $qFName=$entry['file_name'];
      echo "<tr>";
      echo "<td>$qFName</td>";
      
      $result2 = sqlite_array_query($base, "SELECT count(*) as cnt, ip  FROM $mytable  WHERE file_name='$qFName' GROUP BY ip ORDER BY ip",SQLITE_ASSOC,$err_str);
    
      $cnt=0;
      foreach ($result2 as $entry)
	$cnt+=$entry['cnt'];
	
	
      echo "<td><center>" . count($result2) . "</center></td><td><center>$cnt</center></td>";
      echo "</tr>";
   
    }
    echo "</center></table>\n";
    echo "<p>Real downoad = 1 download per IP<BR>";
	
	$res=sqlite_array_query ($base," select distinct ip,count(*) as cnt from $mytable group by ip order by cnt desc", SQLITE_ASSOC, $err_str);
	echo "<table>";
	foreach ($res as $rec)
	{
		echo "<tr>";
		//echo "<td>".$rec['ip'] . " : </td><td>" . $rec['cnt']."</td><td>".GetCityFromIP($rec['ip'])."</td>";
		echo "<td>".$rec['ip'] . " : </td><td>" . $rec['cnt']."</td>";
		//echo "<td>".$rec['ip'] . " : </td><td>" . $rec['cnt']."</td>";
	}
	echo "</table>";
  }
  

  
?>