# Release Process
To create a new Release do the following steps:
- Create a new release in GitHub with the same version
- Bump the version field in the `PKGBUILD` file
- After the release pipeline published its binaries, push the newest `PKGBUILD` to the [AUR](#publish-to-aur).

## [Publish to AUR](https://wiki.archlinux.org/title/AUR_submission_guidelines#Publishing_new_package_content)
```bash
$ makepkg --printsrcinfo > .SRCINFO
$ git add -f PKGBUILD .SRCINFO
$ git commit -m "useful commit message"
$ git push
```

## Changing PKGBUILD
- In case the PKGBUILD needs to be changed, increment the pkgrel field.
