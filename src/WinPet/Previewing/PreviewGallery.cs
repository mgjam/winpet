using System.Net;
using System.Text;
using WinPet.Domain;

namespace WinPet.Previewing;

internal static class PreviewGallery
{
    public static void Write(string directory, IPet pet, IReadOnlyList<PreviewScene> scenes)
    {
        var html = new StringBuilder($$"""
            <!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width">
            <title>{{WebUtility.HtmlEncode(pet.Name)}} visual reference</title>
            <style>body{font:16px system-ui;margin:32px auto;padding:0 24px;max-width:1100px;color:#244635;background:#faf8f1}h1{margin-bottom:8px}p{line-height:1.6}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(245px,1fr));gap:20px}article{background:#f5f2e8;border:1px solid #d9dfd0;border-radius:12px;padding:16px}img{display:block;margin:auto;max-width:100%}h3{margin:12px 0 4px}article p{font-size:14px}a{color:#356e69}button{font:inherit;padding:8px 14px;margin-right:8px;cursor:pointer}nav{position:sticky;top:0;background:#faf8f1;padding:12px 0}</style>
            <h1>{{WebUtility.HtmlEncode(pet.Name)}} visual reference</h1><p>Generated from the production renderer during <code>dotnet build src/WinPet.sln</code>. PNGs are native {{pet.Size.Width}} × {{pet.Size.Height}} pixels; animations show 2× artwork at real-time speed, 20 fps. The ring indicates cursor direction.</p>
            <p>Physical state takes priority over activities. Blinking and cursor attention are independent extras. Clips hold the pet in place to review artwork; they do not simulate desktop collisions. Activity transition clips shorten the settled hold to fit an 8-second loop; production durations are unchanged.</p>
            <nav><button onclick="setAnimated(true)">Play animations</button><button onclick="setAnimated(false)">Show stills</button><a href="overview.png">Overview sheet</a></nav>
            """);
        var md = new StringBuilder($"# {pet.Name} visual reference\n\nGenerated automatically by `dotnet build src/WinPet.sln`, or `tools\\generate-previews.cmd`. Open [the gallery](index.html) for animated cards and still/animation controls. These files belong in source control; regenerate and review them with artwork changes. Do not edit them manually.\n\nPNGs: native {pet.Size.Width} × {pet.Size.Height}, transparent. GIFs: 2× artwork on cream, 20 fps, eight-second loops. The ring represents cursor direction. Previews show artwork in place, not a desktop physics simulation. Activity transition clips use a shortened hold; production durations are unchanged.\n\n![Overview](overview.png)\n");
        foreach (var group in scenes.GroupBy(s => s.Group))
        {
            html.Append($"<h2>{group.Key}</h2><div class=grid>");
            md.Append($"\n## {group.Key}\n\n");
            foreach (var scene in group)
            {
                html.Append($"<article><img width=240 height=208 style=object-fit:contain data-id='{scene.Id}' src='{scene.Id}.gif' alt='{WebUtility.HtmlEncode(scene.Title)}'><h3>{scene.Title}</h3><p>{scene.Description}</p><a href='{scene.Id}.png'>PNG</a> · <a href='{scene.Id}.gif'>GIF</a></article>");
                md.Append($"- **{scene.Title}** — {scene.Description} [PNG]({scene.Id}.png) · [GIF]({scene.Id}.gif)\n");
            }
            html.Append("</div>");
        }
        html.Append("<script>function setAnimated(on){document.querySelectorAll('img[data-id]').forEach(img=>{img.src=img.dataset.id+(on?'.gif':'.png');img.style.imageRendering=on?'auto':'pixelated';})}</script></html>");
        File.WriteAllText(Path.Combine(directory, "index.html"), html.ToString());
        File.WriteAllText(Path.Combine(directory, "README.md"), md.ToString());
    }
}
