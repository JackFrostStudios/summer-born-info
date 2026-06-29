# Prototype Sharing

Use [Surge](https://surge.sh/) when you want to temporarily publish one of the static prototype folders for feedback without setting up a full deployment pipeline.

## Milestone 8 Homepage Prototypes

The approved milestone 8 visual references are:

- `UI/prototypes/stitch_summer_born_school_guide/`
- `UI/prototypes/stitch_summer_born_school_guide_dark_mode/`

From the repository root, move into the prototype folder you want to share:

```powershell
cd UI/prototypes/stitch_summer_born_school_guide
```

Publish it with `npx` so Surge is not installed globally:

```powershell
npx surge . summer-born-homepage-prototype.surge.sh
```

Notes:

- On the first run, Surge may prompt you to create an account or log in.
- Replace `summer-born-homepage-prototype.surge.sh` with any free Surge subdomain that is available.
- Re-running the same command deploys the current folder contents to the same URL.

If you prefer to avoid changing directories, run the same deployment from the repository root:

```powershell
npx surge .\UI\prototypes\stitch_summer_born_school_guide summer-born-homepage-prototype.surge.sh
```

## Teardown

When you no longer want the prototype online, remove the published Surge project:

```powershell
npx surge teardown summer-born-homepage-prototype.surge.sh
```

If you published to a different subdomain, replace `summer-born-homepage-prototype.surge.sh` with that domain.
