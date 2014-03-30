<?
echo $_SERVER['SERVER_NAME'];
if($_SERVER['SERVER_NAME']=="localhost")
    $dbname='../db_stats/Routeur_Dwnl.sqlite';
else
    $dbname='../data/DB/Routeur_Dwnl.sqlite';
$mytable ="dwnload";

$base= sqlite_open($dbname, 0666, $err);
if ($err)  exit($err);

sqlite_query($base,"DROP TABLE $mytable");

$query = "CREATE TABLE $mytable (
	    file_name text,
	    ip text,
            dwnload_date datetime    
            )";
            
$results = sqlite_query($base,$query);

?>