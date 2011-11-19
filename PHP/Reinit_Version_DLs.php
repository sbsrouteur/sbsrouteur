<?
echo $_SERVER['SERVER_NAME'];
if($_SERVER['SERVER_NAME']=="localhost")
    $dbname='../db_stats/Routeur_Dwnl.sqlite';
else
    $dbname='../data/DB/Routeur_Dwnl.sqlite';
$mytable ="dwnload";
$CurVersion = "25.1";

$base= sqlite_open($dbname, 0666, $err);
if ($err)  exit($err); 

$results=sqlite_query($base,"UPDATE $mytable set file_name =\"RouteurV".$CurVersion.".exe\" where file_name=\"Routeur.exe\"");

?>