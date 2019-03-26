$picspath = "C:\Users\ttomow\OneDrive\Bilder\alerts2\video\RQGBG\grabs"

$pics = Get-ChildItem -Path $picspath -Filter "*.jpg" | Select-Object -Skip 2000 
Write-Host "$($pics.Length) images found"

New-Item "R:\Temp\post" -ItemType Directory

foreach ($item in $pics) {
    $result = Invoke-RestMethod -Method Post -Uri http://127.0.0.1:8081/image -ContentType "application/octet-stream" -InFile $item.FullName
    if ($result.predictions.tagName -eq "Postauto" ) {
        $auto = $result.predictions | % {if ($_.tagName -eq "Postauto") { $_.probability}} 
        if ($auto -gt 0.4) {
            Write-Host "$($item.FullName) -> $auto %"
            Copy-Item -Path $item.FullName -Destination "r:\temp\post\$($item.Name)"
        }
    }
}
