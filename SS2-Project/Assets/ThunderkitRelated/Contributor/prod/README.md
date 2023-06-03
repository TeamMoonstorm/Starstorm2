# S T A Y C U T E . . . . .


## post-build build fixes

- Remove R2API DLL
- Add manifest
    ```powershell
        New-Item -Path 'C:\Users\User\Desktop\e\Starstorm2\SS2-Project\ThunderKit\AssetBundleStaging\AssetBundleStaging.manifest' -ItemType File
    ```
- Generate Solution in `Edit > Preferences > External Tools > Regenerate Project Files`
- Check for `MMHOOK_CSHARP` (delete if true)
- Add in `Starstorm.csproj`
    ```xml
    <Target Name="CopyDLL" AfterTargets="AfterBuild">
        <Copy SourceFiles="C:\Users\User\Desktop\e\Starstorm2\SS2-Project\Temp\bin\Debug\Starstorm2.dll" DestinationFolder="C:\Users\User\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\ss2dev\BepInEx\plugins\Starstorm2\plugins" />
    </Target>
    ```
- Paste Assetbundles from build to target (if changed)

## honestly
- yeah idk guys like im trying my best to stay positive and get this done but 
- idk if its thunderkit and all the abstraction layers getting to me, how i was treated as a free volunteer or just an unfortunate sequence of misfortunes but working ss2 kinda sucked all passion i had for ror2 modding lmfao
- like school was definitely 1 thing and getting an actual job was also 1 thing but i did definitely took a break from ror2 and more specifically ss2
-  i couldve definitely crammed everything in if i had the kind of love i had for early rm days i am The Releaser like lol
- no hate to yall obviously, especially zenith ur super cool :3