$content = Get-Content -Raw "src\BringYourOwnAI.UI\Views\ChatWindowControl.xaml"
$content = $content -replace 'Background="\{StaticResource VsWindowBackground\}"', 'Background="{DynamicResource {x:Static SystemColors.WindowBrushKey}}" Foreground="{DynamicResource {x:Static SystemColors.WindowTextBrushKey}}"'
$content = $content -replace 'Background="\{StaticResource VsHeaderBackground\}"', 'Background="{DynamicResource {x:Static SystemColors.ControlBrushKey}}"'
$content = $content -replace 'Foreground="\{StaticResource VsWindowText\}"', ''
$content = $content -replace 'Foreground="White"', ''
$content = $content -replace 'Background="#2D2D2D"', ''
$content = $content -replace 'Background="#2D2D30"', 'Background="{DynamicResource {x:Static SystemColors.ControlBrushKey}}"'
$content = $content -replace 'Background="#252526"', ''
$content = $content -replace 'BorderBrush="#3F3F46"', 'BorderBrush="{DynamicResource {x:Static SystemColors.ControlDarkBrushKey}}"'
Set-Content "src\BringYourOwnAI.UI\Views\ChatWindowControl.xaml" $content
