# Scrap Pickup settings

Settings are in `BepInEx\config\com.dev.survivallog.scrappickup.cfg`, which is created the first time the game starts with the mod. You can change them in game on the Mod Manager's Mods screen, where changes apply right away, or edit the file while the game is closed. Delete the file to get the defaults back.

## Scrap

| Setting | Default | What it does |
| --- | --- | --- |
| `Enabled` | `true` | Turns Scrap Pickup on and off. While it is `false`, items from furniture you scrap or demolish drop the way they do without the mod. |
| `ToTrunk` | `true` | While exploring, items from furniture you scrap or demolish go to your trunk or shopping cart first, then your backpack. Set it to `false` to skip the trunk, so items go straight to your backpack. At home the trunk is never used. Items drop on the ground only when nothing has room. |
