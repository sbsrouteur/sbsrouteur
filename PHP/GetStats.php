<?php

  if($_SERVER['SERVER_NAME']=="localhost")
    $dbname='../db_stats/base_dwnload.sqlite';
  else
    $dbname='../data/DB/Routeur_Dwnl.sqlite';
 
    $mytable ="dwnload";

  $base= sqlite_open($dbname, 0666, $err);
  if (!$err)
  {
    $result = sqlite_array_query($base, "SELECT  file_name,ip, count(*) as cnt FROM $mytable group by file_name,ip",SQLITE_ASSOC,$err_str);
   
    
    foreach($result as $entry)
    {
      echo $entry['file_name'].";".$entry['ip'].";".$entry['cnt']."\n";
      
    }
    
  }
  

  
?>