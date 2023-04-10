
## Building

If the project is open in Unity, use menu `Build` -> target platform.

If the project is not currently open in unity, you can start Unity build by running:

```
> <unity executable path> -projectPath <project path> -executeMethod Builder.BuildWebGL
```

On Linux, if running in the project dir:

```shell
$ /Applications/Unity/Hub/Editor/2021.3.22f1/Unity.app/Contents/Linux/Unity -projectPath . -executeMethod Builder.BuildWebGL
```

On Windows, if running in the project dir:

```shell
> "C:\Program Files\Unity\Hub\Editor\2021.3.22f1\Editor\Unity.exe" -projectPath . -executeMethod Builder.BuildWebGL
```

```powershell
>  & 'C:\Program Files\Unity\Hub\Editor\2021.3.22f1\Editor\Unity.exe' -projectPath . -executeMethod Builder.BuildWebGL
```

## Deploying

With Firebase [configured](https://firebase.google.com/docs/cli):

```
$ firebase deploy
```