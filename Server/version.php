<?php
/**
 * Date: 01-Jul-16
 * Time: 18:49
 */

$dir = './mods';

$modpackUpdated = 0;
$mods = array();
/**
 * Scan mods dir
 */
foreach (array_diff(scandir($dir), array('..', '.')) as $mod) {
    $modFiles = array_diff(scandir("{$dir}/{$mod}"), array('..', '.'));

    $entry = array_pop($modFiles);
    $changed = filemtime("{$dir}/{$mod}/{$entry}");
    foreach ($modFiles as $modFile) {
        $modFileChanged = filemtime("{$dir}/{$mod}/{$modFile}");
        if ($modFileChanged > $changed) {
            $entry = $modFile;
            $changed = $modFileChanged;
        }
    }

    $file = "{$dir}/{$mod}/{$entry}";
    $mods[] = array(
        'name' => $mod,
        'file' => $entry,
        'updated' => $changed,
        'size' => filesize($file),
        'checksum' => md5_file($file)
    );

    $modCreated = filemtime("{$dir}/{$mod}");
    $modpackUpdated = $changed > $modpackUpdated ? $changed : $modpackUpdated;
    $modpackUpdated = $modCreated > $modpackUpdated ? $modCreated : $modpackUpdated;
}

header('Content-type: text/json');
print json_encode(array(
    'updated' => $modpackUpdated,
    'mods' => $mods
));