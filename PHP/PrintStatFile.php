<?
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
		echo "<td>".$rec['ip'] . " : </td><td>" . $rec['cnt']."</td>";
	}
	echo "</table>";
  }
?>