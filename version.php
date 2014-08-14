<?php

	function adddir($dir, $base) {
		$files = scandir($base.$dir);
		$md5files = array();
	        foreach ($files as $file)
	        {
	                if ($file == "." || $file == "..")
	                        continue;
	                if (is_dir($file)) 
	                {
	                	adddir($file);        
	                } else {
	                        array_push($md5files, array('file' => $dir.'\\'.$file, 'md5sum' => md5_file($base.$dir.'/'. $file)));
	                }

	        }	
		return $md5files;
	}

	$files = scandir("files/");

	$md5files = array();

	foreach ($files as $file)
	{
		if ($file == "." || $file == "..")
			continue;
		if (is_dir('files/' . $file))
		{
			$arr = adddir($file, 'files/');
			foreach ($arr as $entry) {
				array_push($md5files, $entry);
			}
		} else {
			array_push($md5files, array('file' => $file, 'md5sum' => md5_file('files/' . $file)));
		}

	}

	$version = array('version' => '0.4.0.4', 'files' => $md5files);

echo json_encode($version, JSON_PRETTY_PRINT);
