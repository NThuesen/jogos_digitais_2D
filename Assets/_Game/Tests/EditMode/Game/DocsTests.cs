using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neko.Sim.Economy;
using Neko.Sim.Maids;
using Neko.Sim.Service;
using Neko.Sim.Team;
using NUnit.Framework;
using UnityEngine;

namespace Neko.Tests.Game
{
    /// <summary>
    /// The documentation the brief grades, checked the way code is checked.
    /// </summary>
    /// <remarks>
    /// The GDD is required to be a living document, which in practice means it is required
    /// not to go stale. The failure it is actually exposed to is not a missing section — it
    /// is a maid or a customer added to a table in Sim and never written down. That is the
    /// one thing a test can notice, so these read the tables and look for each name in the
    /// document, and fail with the names that are missing.
    /// </remarks>
    public sealed class DocsTests
    {
        private static string Root =>
            Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        private static string Read(string relative)
        {
            var path = Path.Combine(Root, relative);
            Assert.IsTrue(File.Exists(path), relative + " is missing");
            return File.ReadAllText(path);
        }

        /// <summary>The twelve headings of the course rubric, in the brief's own order.</summary>
        private static readonly string[] Rubric =
        {
            "## 1. Blueprint",
            "## 2. Mechanics",
            "## 3. Feedback",
            "## 4. UI/HUD",
            "## 5. Engajamento",
            "## 6. Level design",
            "## 7. Design de som",
            "## 8. Efeitos visuais",
            "## 9. Rejogabilidade",
            "## 10. Balanceamento",
            "## 11. Gerenciamento de estados",
            "## 12. Este documento",
        };

        [Test]
        public void TheGddCoversEveryRubricHeading()
        {
            var gdd = Read("docs/GDD.md");
            var missing = Rubric.Where(h => !gdd.Contains(h)).ToList();

            Assert.IsEmpty(missing, "GDD.md has no section for: " + string.Join(", ", missing));
        }

        [Test]
        public void EveryArchetypeIsDocumentedInTheGdd()
        {
            var gdd = Read("docs/GDD.md");

            var missing = ArchetypeTable.All
                .Where(a => !gdd.Contains(a.DisplayName))
                .Select(a => a.DisplayName)
                .ToList();

            Assert.IsEmpty(missing,
                "these maids exist in ArchetypeTable and not in the GDD: " +
                string.Join(", ", missing));
        }

        [Test]
        public void EveryCustomerTypeIsDocumentedInTheGdd()
        {
            var gdd = Read("docs/GDD.md");

            var missing = CustomerTable.All
                .Where(c => !gdd.Contains(c.DisplayName))
                .Select(c => c.DisplayName)
                .ToList();

            Assert.IsEmpty(missing,
                "these customers exist in CustomerTable and not in the GDD: " +
                string.Join(", ", missing));
        }

        [Test]
        public void EveryPositionAndEveryRarityIsDocumentedInTheGdd()
        {
            var gdd = Read("docs/GDD.md");

            var missing = new List<string>();
            missing.AddRange(RoleTable.All.Select(r => r.DisplayName).Where(n => !gdd.Contains(n)));
            missing.AddRange(RarityTable.All.Select(r => r.DisplayName).Where(n => !gdd.Contains(n)));

            Assert.IsEmpty(missing, "not in the GDD: " + string.Join(", ", missing));
        }

        [Test]
        public void TheReadmeCreditsTheOneImportedSuzuSheet()
        {
            var readme = Read("README.md");

            Assert.IsTrue(readme.Contains("Créditos de assets"),
                "the brief asks for an asset-credit section and the README has none");
            Assert.IsTrue(readme.Contains("Assets/_Game/Resources/Suzu/suzu_walk_sheet.png"),
                "the README no longer credits the imported Suzu sheet");
            Assert.IsTrue(readme.Contains("docs/art/SUZU_ASSET_PROVENANCE.md"));
            Assert.IsFalse(readme.Contains("Não há nenhum"),
                "the credits section claims nothing was imported while a sheet is");
        }

        [Test]
        public void EveryImportedAssetIsOnTheClosedProvenanceAllowlist()
        {
            // The credits claim is only true while the list of imported files is exactly the one
            // written down with its hash. Resources/Audio is excluded: those .wav files are
            // written by tools/synthesise-audio.sh from this repository, not imported.
            var provenance = Read("docs/art/SUZU_ASSET_PROVENANCE.md");
            var expected = provenance.Split('\n')
                .Select(line => line.TrimEnd('\r'))
                .Where(line => line.Length > 66 && line[64] == ' ' && line[65] == ' ')
                .ToDictionary(line => line.Substring(66).Trim(), line => line.Substring(0, 64));

            CollectionAssert.AreEquivalent(new[] { "Assets/_Game/Resources/Suzu/suzu_walk_sheet.png" },
                expected.Keys);

            var synthesised = Path.Combine(Application.dataPath, "_Game", "Resources", "Audio")
                .Replace('\\', '/') + "/";

            var imported = Directory
                .EnumerateFiles(Application.dataPath, "*", SearchOption.AllDirectories)
                .Where(p => !p.Replace('\\', '/').StartsWith(synthesised))
                .Where(p =>
                {
                    var ext = Path.GetExtension(p).ToLowerInvariant();
                    return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".psd" ||
                           ext == ".tga" || ext == ".gif" || ext == ".bmp" || ext == ".wav" ||
                           ext == ".mp3" || ext == ".ogg" || ext == ".aiff" || ext == ".ttf" ||
                           ext == ".otf" || ext == ".fbx";
                })
                .Select(p => p.Substring(Root.Length + 1).Replace('\\', '/'))
                .ToList();

            CollectionAssert.AreEquivalent(expected.Keys, imported,
                "imported files must match the credited allowlist exactly");

            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                foreach (var entry in expected)
                {
                    var hash = sha.ComputeHash(File.ReadAllBytes(Path.Combine(Root, entry.Key)));
                    Assert.AreEqual(entry.Value,
                        System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant(),
                        entry.Key + " differs from the credited source");
                }
            }
        }

        [Test]
        public void TheItchPageIsWrittenAndStatesTheBoxOdds()
        {
            var page = Read("docs/ITCH_PAGE.md");

            Assert.IsTrue(page.Contains("Neko Maid Inc."), "the page does not name the game");

            // The odds are the one thing on a lootbox page that must not drift from the code.
            foreach (var box in Lootbox.All)
            {
                Assert.IsTrue(page.Contains(box.DisplayName.Replace("Caixa ", "")),
                    box.DisplayName + " is on sale in the game and not on the page");
            }
        }
    }
}
