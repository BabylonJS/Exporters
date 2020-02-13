$collItems =  (Get-ChildItem * -Recurse -Force -Include *.cs)
$collItems | ForEach-Object { 
	$text = [IO.File]::ReadAllText($_) -replace "`r`n", "`n"
	[IO.File]::WriteAllText($_, $text)

	# Replace CR with LF
	$text = [IO.File]::ReadAllText($_) -replace "`r", "`n"
	[IO.File]::WriteAllText($_, $text)
}