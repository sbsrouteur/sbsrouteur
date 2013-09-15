<?

function splitFilename($filename)
{
    $pos = strrpos($filename, '.');
    if ($pos === false)
    { // dot is not found in the filename
    return array($filename, ''); // no extension
    }
    else
    {
        $basename = substr($filename, 0, $pos);
        $extension = substr($filename, $pos+1);
        return array($basename, $extension);
    }
}

  $dir = "./Routeur/";

if (isset($_GET["disp_file"]))
{    
    $fname=$_GET["disp_file"];
    /*if(isset($_GET["mode"]))
    {
		if($_GET["mode"]==1)
			$fname.=".zip";
		else
			$fname.=".exe";
    }
    else
		$fname.=".zip";
	*/	
    if(file_exists($dir.$fname))
    {
		if($_SERVER['SERVER_NAME']=="localhost")
			$dbname='../db_stats/base_dwnload.sqlite';
		else
			$dbname='../data/DB/Routeur_Dwnl.sqlite';
	
		$mytable ="dwnload";

		$base= sqlite_open($dbname, 0666, $err);
		if (!$err)
		{
			  $query =  'INSERT INTO ' . $mytable . ' ' .
				'VALUES ("' . $_GET["disp_file"] . '","' . $_SERVER['REMOTE_ADDR'] . "\",date('now'))";
			  //echo $query ."<br>";
			  sqlite_query($base,$query);
			  //echo "Result: " . $res;
		}

		if(filesize($dir.$fname) <= 67108864)
		{
			  header("Content-type: application/x-file-to-save");
			  header("Content-Disposition: attachment; filename=".$fname);
			  readfile($dir.$fname);
		}
		else
		{        
			  echo "<head>";
			  echo "<meta http-equiv='refresh' content='0;URL=$dir$fname'>";        
			  echo "</head>";
		}

        exit;
    }
}
else
{
 ?>
 <htm>
 <head>
 <title>Routeur Download</title>
 <LINK REL="ICON" HREF="./icon.png">
 </head>
 <body>
 <h1><center>Routeur download (Current and older version) - Windows</center></h1>
 <h2> Current Setup : </h2>
	<a href='index.php?disp_file=Routeur.zip'>Setup Routeur</a>
 <h2> Current exe : </h2>
	<a href='index.php?disp_file=Routeur.exe'>Routeur</a>
 
 <h2> Older Versions and Setups </h2>
 <table width='100%'><tr>
 <?
 $str="";
 
 // Ouvre un dossier bien connu, et liste tous les fichiers

for($pass=0;$pass<2;$pass++)
{
  echo "<td>";
  if($pass==0)
    echo "<h3><center>Full Setup Zipped files</center></h3>";
  else
    echo "<h3><center>Application (.exe)</center></h3>";
    
  if (is_dir($dir)) {
    if ($dh = opendir($dir))
    {
		$liste=array();
        $liste_ext[]=array();
        $i=0;
        while (($file = readdir($dh)) !== false)
        {
            if(is_file($dir . $file))
            {
                $fname_tab=$file; 
                $liste[$i]=$fname_tab;
                $i++;
                
            }
        }
        closedir($dh);
        rsort($liste);
        $current="";
        
        
		echo $liste[0];
        echo "<ul>\n";

        for($j=0;$j<count($liste);$j++)
        {
            /*$myVersion=split("V",$liste[$j]);
            if($myVersion[1]!=$current)
            {
                if($current!="")
                {
                    echo "</ul>\n";
                }
                $current=$myVersion[1];
                echo "<li> Version ".$current."</li>\n<ul>\n";
            }*/
			$fnametab[] = array();
			$fnametab=split("\.",$liste[$j]);
			if ($lists[$j] != "Routeur.exe" && $lists[$j] != "gshhs.7Z")
			{
				$extindex = count($fnametab) -1;
				if($fnametab[$extindex] == "zip" && $pass == 0)
				{
					$mode=1;
					echo "<li><a href='index.php?disp_file=".$liste[$j]."$str'>".$liste[$j]."</a></li>\n";
				}
				elseif ($fnametab[$extindex] == "exe" && $pass==1)
				{
					$mode=2;
					echo "<li><a href='index.php?disp_file=".$liste[$j]."$str'>".$liste[$j]."</a></li>\n";
				}
				/*else
				{
					echo"<li>".$liste[$j]." / ".$extindex.$fnametab[$extindex]."</li>\n";
				}*/
			}
			
        }
        echo "</ul>\n</ul>\n";

    }

  }  
  echo "</td>";
  if($pass==0)
    echo "<td width='1' bgcolor='red'><BR></td>";
}
	
echo "</tr></table>";
}

?>
</body>
</html>