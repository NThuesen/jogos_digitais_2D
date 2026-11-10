# Suzu — imported runtime asset

The textures imported into the player are the Suzu walk sheet the old Suzu sheet kept as a backup, and the nine maid walk sheets of ART-03 (below).

**Origin.** Suzu was designed in this project (ChatGPT image, D-pixel-cafe direction), animated
from a still with Tencent AI Studio, exported to 30 PNG frames with Blender
(`art_working/01_suzu/step4_blender/walk_x/frames/`, git-ignored, never modified) and reduced to
the sheet below by `tools/art/suzu_walk/process.py`. No third-party character or image is
imported. Every parameter and the SHA-256 of each source frame are in
[`docs/art/suzu-walk/manifest.json`](suzu-walk/manifest.json).

**What the tool does:** removes matte streaks, replaces the closed eyes of the video's long
blink with the eyes of the nearest open frame, anchors the planted foot and the skirt axis,
scales every frame by one factor to 120 px of character height, quantises to one shared 48 colour
palette with hard alpha, and packs the 30 cells into one grid. Import settings are Point, no
mipmaps, uncompressed, clamp, non-readable (see `suzu_walk_sheet.png.meta`).

SHA-256 allowlist (also checked by `DocsTests`):

```text
4b7b617482c6ee210a604cc27d65234748892418fc826a991b179336cbf059da  Assets/_Game/Resources/Suzu/suzu_walk_sheet.png
b45fc50515464814243fcac49e110a8411152dd049282fcd80cd975d20e81294  Assets/_Game/Resources/Maids/akari_walk.png
a70b1d5934a7d4a9cee9b324e9c7817b9331b364971ea64dfa09b927e61fc5b8  Assets/_Game/Resources/Maids/hina_walk.png
0e2fea32076ebf949598be2a7edc0426ba4f8a6a27e4d4a69592398f734ffbc1  Assets/_Game/Resources/Maids/ichika_walk.png
d5f7e47ed17d4a996714489858bc087ddcfa34bc3d683ed7caa81fc0b49eb331  Assets/_Game/Resources/Maids/kaede_walk.png
0f40961c552574e5bb32fdd2bb87b02a12e4c611f4e8d7aa1ecfcb7b4ae1466b  Assets/_Game/Resources/Maids/momo_walk.png
fa90374d2ca21b49487fb58f0e9834a7160d28757eb33f5e3e1cebfc0db1087f  Assets/_Game/Resources/Maids/nene_walk.png
1febda572a21fb5de7c32f1e6e56ccf5a9a8db693d872ea2a87e89d7f56c6c64  Assets/_Game/Resources/Maids/reina_walk.png
001cb5b0b2e538fb7979ee9c78185913b0a96bc8573fea4deb4bf32e2820aefb  Assets/_Game/Resources/Maids/yoru_walk.png
8a7f4f09c865c77e878dca5460585bafe02d5f7f3fe78ae75b7458be12caffc7  Assets/_Game/Resources/Maids/suzu_walk.png
be482f46bdb9479c476cdcf11cb6ac8a627ff0acbbacbdfde091614ded9d6c5a  Assets/_Game/Resources/UiArt/btn1_disabled.png
5ae7316ad06a835ee427474f3bd89fe474b48d508334ffa3dbef313d8078dcb2  Assets/_Game/Resources/UiArt/btn1_hover.png
547ea47509b00cfb2f752b5a31d12578bb20231ce54b5adf9bce9319fa86c6fa  Assets/_Game/Resources/UiArt/btn1_normal.png
70ea100d92814aebdbfb0bc48e7facf5f6ac8311c9a19bfcadde0b75d7bf0c07  Assets/_Game/Resources/UiArt/btn1_pressed.png
a53894a6ef00b0d52f9d511b388dcd8bff82f052867b81981ba13c9d216e72fd  Assets/_Game/Resources/UiArt/btn2_disabled.png
c70296433bfcb17c5ae948c7968f8b4cc2b1d260953264ab14a2b779caf278e7  Assets/_Game/Resources/UiArt/btn2_hover.png
49fe2d82a4dbebaed2ca16ab457b6bec74848aa8e53d81fe2450bf00d5b07b98  Assets/_Game/Resources/UiArt/btn2_normal.png
f7fb0934255ddc17346bca012ef36a77023a7f8b1b7b6cb8d4101eca316240f1  Assets/_Game/Resources/UiArt/btn2_pressed.png
1faac4771f2d2777b97ea1f91ada6ffd5cb5a64971ddbb9f11d18c9e5a98809c  Assets/_Game/Resources/UiArt/icon_cat_paw.png
264d56051ff5a5621c40010871af213b8900a9eadb7b3c18922c3bdf32cfe42a  Assets/_Game/Resources/UiArt/icon_coin.png
212828d066a6889cbf27ec0db30cca012173c21de36415a3fb040983f0db3002  Assets/_Game/Resources/UiArt/icon_gift.png
43caf48781ce65991f7502a50dac4c093289be618c8de444d99c47607c560d6d  Assets/_Game/Resources/UiArt/tab_active.png
3d6a545a47da324ad1cdd3edd52f65597e6b004be4d31f4624d777a6d3219d63  Assets/_Game/Resources/UiArt/tab_hover.png
d79f4f76f02df177ae38679e8bae11b6dbc7c6e1bd7ec31f01f3d14e72f81ebb  Assets/_Game/Resources/UiArt/tab_inactive.png
d079d353925245c9e8c7e2891e69d40e19b47678808241fce9e618b67c18c22d  Assets/_Game/Resources/UiArt/ui012_menu_background.png
f1b69db6ed2a8cff93234d16344a05b4e75390ea6974956fe54dd54f58de2035  Assets/_Game/Resources/UiArt/ui017_shop_row.png
39e09b3959fd5a9759160a44d909672a7f26d0cc37877ed907954d29afb20dd0  Assets/_Game/Resources/UiArt/ui020_logo_wordmark.png
1a7bf6407c23cf037674d38b182efb5975e916a5ba3467fad7a6babbf1c4f203  Assets/_Game/Resources/UiArt/card_common.png
5841138f29e2997908a9cd253b9017d3f8c137a56682fb2beda537d941e6dfd5  Assets/_Game/Resources/UiArt/card_rare.png
45a15adbc559ee2cbad89eb053212db4b2ed6b143054bc95be0e549f917136df  Assets/_Game/Resources/UiArt/card_epic.png
6309afc1452e6a78ddd37d74be0de9520c7d3b1eab62a5b4e772290a2fa6c2dd  Assets/_Game/Resources/UiArt/card_legendary.png
0427a384664a16b3572e7657bd1ea8ebb8b7cfa7a20d8472b3a27e57e69a7155  Assets/_Game/Resources/UiArt/card_minimal.png
9d67bfc86ba3547ce22688d767b2c9935973da680422140d31d3106a3d6206e1  Assets/_Game/Resources/UiArt/badge_common.png
138c7ae134953b1be9f47af2ab99a0c6be5dcfd5dae5f2682fc997a69b657992  Assets/_Game/Resources/UiArt/badge_rare.png
4b8d42b0112588000d289839b2223fb3427c170c4940d782fbc766f188feeb10  Assets/_Game/Resources/UiArt/badge_epic.png
9ff83912f3fd13576f34a51859a36e12fc0ffbcafd5523cd54edd54b47d12d2f  Assets/_Game/Resources/UiArt/badge_legendary.png
```

## Maid walk sheets (ART-03, restyled in ART-04)

Suzu (new frames), Hina, Akari, Momo, Reina, Kaede, Nene, Ichika and Yoru were designed in this project (same chain as Suzu:
ChatGPT image, Tencent AI Studio animation, Blender frame export to the git-ignored
`art_working/<NN_maid>/step4_blender/walk_x/frames/`, never modified). Each sheet is produced by
`tools/art/maid_walk/maid_walk.py process` (parameters, per-frame source SHA-256 and the sheet hash are in
`docs/art/maid-walk/<id>/manifest.json`). Since ART-04 the sheets are the illustrated
style (colours, shading and soft alpha kept; ~240 px tall characters; Trilinear, mipmaps, high-quality block compression,
non-readable). The ART-03 pixel-art sheets are commit `37b0f3f` and `maid_walk.py process --style pixel` reproduces them. The hashes are in the allowlist block above, which `DocsTests` checks.

`Resources/Suzu/suzu_walk_sheet.png` (PR #27) is no longer used by the catalog; it stays only as a rollback backup until the owner approves deleting it. See `docs/art/maid-walk/INVENTORY.md`.

## Modular UI art (UI integration)

The `Assets/_Game/Resources/UiArt/*.png` files are owner-approved UI sprites copied byte-for-byte from `docs/art/ui-modular/approved/`; each source path and hash is in `Assets/_Game/Resources/UiArt/SOURCES.json`. Their hashes are in the allowlist block above, which `DocsTests` checks.

## Premium maid portraits (UI-021)

The nine owner-approved portraits (APPROVED, with the Suzu and Ichika edge notes in
`docs/art/ui-modular/portraits/RESULTADO.md`). `Resources/MaidPortraits/<id>_head.png` (192x157, Maid Cards)
and `<id>_detail.png` (480x641, Elenco detail) are byte-for-byte the derivatives of
`tools/art/maid_portraits_prepare.py`, whose SHA-256 with the RAW and CLEAN hashes is in
`docs/art/ui-modular/portraits/MANIFEST.json`. The same bytes stay under `Tests/PlayMode/QaArt` for the QA test.
No new image was generated for the integration.

```text
d90ccec1dc103369df9c6b5c97d709af8db9fbac2787cc700b6946eeefa46a9b  Assets/_Game/Resources/MaidPortraits/suzu_head.png
d90ccec1dc103369df9c6b5c97d709af8db9fbac2787cc700b6946eeefa46a9b  Assets/_Game/Tests/PlayMode/QaArt/suzu_head_192.png
8550a8012ee966d2664727f349c0a66f7fdf15e44314128e6a4ebe0a3650a45c  Assets/_Game/Resources/MaidPortraits/suzu_detail.png
8550a8012ee966d2664727f349c0a66f7fdf15e44314128e6a4ebe0a3650a45c  Assets/_Game/Tests/PlayMode/QaArt/suzu_detail_480.png
7740feed27c36bb142f39bf0ebf49949800eb813fdb3c13168b0b40375fae29c  Assets/_Game/Resources/MaidPortraits/hina_head.png
7740feed27c36bb142f39bf0ebf49949800eb813fdb3c13168b0b40375fae29c  Assets/_Game/Tests/PlayMode/QaArt/hina_head_192.png
a42570b4a1b58e9d870ca83c17bb89a86bf72f885ad272864f6033d935dc1747  Assets/_Game/Resources/MaidPortraits/hina_detail.png
a42570b4a1b58e9d870ca83c17bb89a86bf72f885ad272864f6033d935dc1747  Assets/_Game/Tests/PlayMode/QaArt/hina_detail_480.png
7bd0c288acf868dbb04f4d2df70b36df240963f1e3719e81bedc850bfc4b1020  Assets/_Game/Resources/MaidPortraits/akari_head.png
7bd0c288acf868dbb04f4d2df70b36df240963f1e3719e81bedc850bfc4b1020  Assets/_Game/Tests/PlayMode/QaArt/akari_head_192.png
331a0b151c5101c18e5117fef73206fbdf88c09c2e5c89acafa487c06ea23cf8  Assets/_Game/Resources/MaidPortraits/akari_detail.png
331a0b151c5101c18e5117fef73206fbdf88c09c2e5c89acafa487c06ea23cf8  Assets/_Game/Tests/PlayMode/QaArt/akari_detail_480.png
17eacc684d38a109a5c1bf5c8174a0dcd45a3f22bdd85ef7b43ea3a4f1a76826  Assets/_Game/Resources/MaidPortraits/momo_head.png
17eacc684d38a109a5c1bf5c8174a0dcd45a3f22bdd85ef7b43ea3a4f1a76826  Assets/_Game/Tests/PlayMode/QaArt/momo_head_192.png
def4e9cd12579dbcf3cca6c6da0ed80e92c7c49c378f17fa8c0714ad47defaaf  Assets/_Game/Resources/MaidPortraits/momo_detail.png
def4e9cd12579dbcf3cca6c6da0ed80e92c7c49c378f17fa8c0714ad47defaaf  Assets/_Game/Tests/PlayMode/QaArt/momo_detail_480.png
ca261c716b692b4a58633c25bff660212b8c0f147ae49c6a9bccddd16edb6369  Assets/_Game/Resources/MaidPortraits/reina_head.png
ca261c716b692b4a58633c25bff660212b8c0f147ae49c6a9bccddd16edb6369  Assets/_Game/Tests/PlayMode/QaArt/reina_head_192.png
6b924941b8ecfd3cca43e3c349172aa555f72cedc37fe50c989f8ba9bff9cb09  Assets/_Game/Resources/MaidPortraits/reina_detail.png
6b924941b8ecfd3cca43e3c349172aa555f72cedc37fe50c989f8ba9bff9cb09  Assets/_Game/Tests/PlayMode/QaArt/reina_detail_480.png
8e2120e1aa31b6de46eceddc7fd70acd57cdac758b5f07ce30fb7485eff2526c  Assets/_Game/Resources/MaidPortraits/kaede_head.png
8e2120e1aa31b6de46eceddc7fd70acd57cdac758b5f07ce30fb7485eff2526c  Assets/_Game/Tests/PlayMode/QaArt/kaede_head_192.png
9f93ba8c0e40cf01709263649d5ed912531ac29063daf186912d2bb560209b34  Assets/_Game/Resources/MaidPortraits/kaede_detail.png
9f93ba8c0e40cf01709263649d5ed912531ac29063daf186912d2bb560209b34  Assets/_Game/Tests/PlayMode/QaArt/kaede_detail_480.png
76365fd3d9f7599260be56de51abe2867df0fa0d321fc797fb67d4d128df0fa1  Assets/_Game/Resources/MaidPortraits/nene_head.png
76365fd3d9f7599260be56de51abe2867df0fa0d321fc797fb67d4d128df0fa1  Assets/_Game/Tests/PlayMode/QaArt/nene_head_192.png
7709f54d676091696d1bb514144aa658c45c28bd4f3f85317f71f80a01f9981e  Assets/_Game/Resources/MaidPortraits/nene_detail.png
7709f54d676091696d1bb514144aa658c45c28bd4f3f85317f71f80a01f9981e  Assets/_Game/Tests/PlayMode/QaArt/nene_detail_480.png
40931dad831ffa245b5c808242eeb41b2a6f586b7a875c41e18743b1905aa6be  Assets/_Game/Resources/MaidPortraits/ichika_head.png
40931dad831ffa245b5c808242eeb41b2a6f586b7a875c41e18743b1905aa6be  Assets/_Game/Tests/PlayMode/QaArt/ichika_head_192.png
8d368093797b9fb18947b8b99600872b2626d7b320591f2ccaf6335a2a1bcdca  Assets/_Game/Resources/MaidPortraits/ichika_detail.png
8d368093797b9fb18947b8b99600872b2626d7b320591f2ccaf6335a2a1bcdca  Assets/_Game/Tests/PlayMode/QaArt/ichika_detail_480.png
544b699a018492cbc379a39b9293f7e3da08c58410f2c1e2dd7d564ebeea591d  Assets/_Game/Resources/MaidPortraits/yoru_head.png
544b699a018492cbc379a39b9293f7e3da08c58410f2c1e2dd7d564ebeea591d  Assets/_Game/Tests/PlayMode/QaArt/yoru_head_192.png
0675e1fdf6ba67322d8e89cb14796d77675c957855973c6227d0eafa1b3d3f4e  Assets/_Game/Resources/MaidPortraits/yoru_detail.png
0675e1fdf6ba67322d8e89cb14796d77675c957855973c6227d0eafa1b3d3f4e  Assets/_Game/Tests/PlayMode/QaArt/yoru_detail_480.png
0c818fc62aeba30ae367cfc42fe800337fe2f6395796612dc58e687f2d43faea  Assets/_Game/Resources/Maids/mikan_walk.png
9b10def9b4caa3dfa9ae0d5701d832acd87bb00845619a5cb5a42a90f4ef7830  Assets/_Game/Resources/MaidPortraits/mikan_head.png
83a816c505725a1495258352c0fa5be8983c644ccfc9a4d78280bcc8dcf307ac  Assets/_Game/Resources/MaidPortraits/mikan_detail.png
39e6936ad833d857bf35fb6a1f7b3a5d330b515df7105cb877e73d7ba7ef79f8  Assets/_Game/Resources/Maids/yuki_walk.png
96264718c395007b0c9424a7b9c96a5e0b4a2214d4a0ece3a4ac6ebc5ad54209  Assets/_Game/Resources/MaidPortraits/yuki_head.png
77db3c767f5b4e9fab573c0fb1db538f14ec240b60652207dfeb42a93084243b  Assets/_Game/Resources/MaidPortraits/yuki_detail.png
f8d59ef0cbc1ff34c9d35fa135cce47617b293a6a3686d8297f7c24c71080376  Assets/_Game/Resources/Maids/kurumi_walk.png
e409265436f1faea8456dc80d1a7cc3e422d7106320521cfdbc98d101257650b  Assets/_Game/Resources/MaidPortraits/kurumi_head.png
a73f86bbf93d0c2b2a72348428e2481343c80f4d530e32cbc6b1bd4f04370b03  Assets/_Game/Resources/MaidPortraits/kurumi_detail.png
6bb710b66b5cda5ccd1f8fb26d04a89d550a3229c728aa824f8164eb9ebb6c2f  Assets/_Game/Resources/Maids/nonoka_walk.png
e6db6cbc9fecc240fcf6e915125772c2c1edeb15120ff012ab08627eb0ca5b49  Assets/_Game/Resources/MaidPortraits/nonoka_head.png
becb91f9cdf4faeb240c370f4860e3fcc8509f00b326abcf28d2229ba70cee42  Assets/_Game/Resources/MaidPortraits/nonoka_detail.png
120599a3eeed768cfb91f5049ec63e2acb8fb1feb46b6cb861b164d8f61ea03c  Assets/_Game/Resources/Maids/kaguya_walk.png
10b0c8657cf3b989699c865fd1cc6ffb649e8b2bd56f6f65e577971d8dcc80e2  Assets/_Game/Resources/MaidPortraits/kaguya_head.png
21194962f03c3df4ea8f03c354d053ea90998a4fc6545deac520a73e5c355252  Assets/_Game/Resources/MaidPortraits/kaguya_detail.png
88396cb9db6dbaf5e5e0fbd153a2ac0a8a368f2125c52218b496703f426ea6bd  Assets/_Game/Resources/Maids/mizuki_walk.png
e7702eb1070dc13e29a2773ae57d3e708e701aa8e1b9c5f67150e34fbfc137cc  Assets/_Game/Resources/MaidPortraits/mizuki_head.png
db580cb0d4c54164bc19d3ef51bd020e9a930f508c34ca3b62ce4a38f32c33fe  Assets/_Game/Resources/MaidPortraits/mizuki_detail.png
1a5be37337637c0e1b25381c721fc2926b0f473416905296b4b5489ec02ca235  Assets/_Game/Resources/Maids/himari_walk.png
c264ddc30dc82d7f4467659b38ce6fbc29fbd830347aae544a9a225d68ec52f5  Assets/_Game/Resources/MaidPortraits/himari_head.png
ad5f6d5d9c4c360ae2188ff061e7d783c75ff53c4b5b2565ecd1a3d1be71a70c  Assets/_Game/Resources/MaidPortraits/himari_detail.png
e27616cae6da7d734996d57f0bacede96802733ef2fa8cf7b9640dbd0259a9eb  Assets/_Game/Resources/Maids/anzu_walk.png
9ed0e57357184b2a3544350b19ca757e4a3def7bc22f220d2d9ae8dbfb8c11d4  Assets/_Game/Resources/MaidPortraits/anzu_head.png
6a7d8ba1de1a8693e882cd09da9138586e81ceaa27cc928373ec04f32d3cd8f3  Assets/_Game/Resources/MaidPortraits/anzu_detail.png
326f70e4780037a93f0c547f082d867e1a95cdd6461e82041e6e95673d2ae052  Assets/_Game/Resources/Maids/kasumi_walk.png
4506e3a66028ab644f89bf1dccea503acaaa418a3dea01fb775af3e656770440  Assets/_Game/Resources/MaidPortraits/kasumi_head.png
5d89eafbb5ff9ab719310fa1e72fa744d97ed0438bd2c3a9f3fe085b3942406f  Assets/_Game/Resources/MaidPortraits/kasumi_detail.png
30bd81f5b9b0eca781784ac181920fd0bb27e036ba13f4ba5f639ed3d8d7f0a0  Assets/_Game/Resources/Maids/hibari_walk.png
7f37546e9781f87e57f1055a1552fe01d2bffe74c14ea91327cd2f8a02c0f65c  Assets/_Game/Resources/MaidPortraits/hibari_head.png
9ca8d8526bb170c316be760cdae8a8a7e8744e8e22bd6681649fce560ffd3a08  Assets/_Game/Resources/MaidPortraits/hibari_detail.png
9e3f113f7311090a1da5e6103e0854cfa46e901d39ebcbfd45c04717bc510a13  Assets/_Game/Resources/Maids/seira_walk.png
699c50522dc1eae72e5ac1e8022e9ceaf4c4c85774411e372f6396adb94a10b6  Assets/_Game/Resources/MaidPortraits/seira_head.png
68cc696dea09a6422f63238f9c7409a36cd652f408fd158de650ca46702b1a98  Assets/_Game/Resources/MaidPortraits/seira_detail.png
ae411eedc8463edfb38e9240c478faf2c3d979f914db53fe43160fd3342be9ae  Assets/_Game/Resources/Maids/ruri_walk.png
fe848e932c9a9d07a7b04ac86fd9ebd95de79c86af2ace011f7dbfaee0e360a3  Assets/_Game/Resources/MaidPortraits/ruri_head.png
da6f3aae179664d137a6e7ac68073b261e609815a841e6a002d0417479d1b710  Assets/_Game/Resources/MaidPortraits/ruri_detail.png
81dacc6d530d08a1163c2a3aba66b7267f4faa9b5adf626c12e18e07e0ef8e44  Assets/_Game/Resources/Maids/chiyo_walk.png
c57bc6d6ae302291fc3c696d78c808623a11797fc07eb7f8caa278711e7ed25d  Assets/_Game/Resources/MaidPortraits/chiyo_head.png
b06a35e7ba8ffb9dcbdd70959bcc0fbea3658538908705a8cf92e1411b0b0589  Assets/_Game/Resources/MaidPortraits/chiyo_detail.png
4ea0945280746e99b112b7aeaee6d47aead47e8fc9fe5a56031cee4788e88eb8  Assets/_Game/Resources/Maids/shiori_walk.png
19511f487448d37f9664cffc40320ce89d4f20d05ace472d866df35ba5660984  Assets/_Game/Resources/MaidPortraits/shiori_head.png
5bc91db7a21b84cccceb5f6df9a6838555ae6f8aede969893b9131d33f83afc5  Assets/_Game/Resources/MaidPortraits/shiori_detail.png
da547dc3938d2b9697618b7e8beed9c262c2d527ab0eb6fd6ab8946a6a16be24  Assets/_Game/Resources/Furniture/koi_pond.png
010ad078ba5a2955c382a74ccceab894f7827aec96669e93bd6cb6897d6c7fb5  Assets/_Game/Resources/Furniture/koi_pond_raised.png
3ba23f7ff1ffc26f0712829dc1381edccc49eed29ebb68d2072f513b5330407b  Assets/_Game/Resources/Furniture/stage.png
1821aa44e65f326d6f81541f4408cd845680be47834f1612e6a9dabe6e40665d  Assets/_Game/Resources/Furniture/window.png
```

## ART-INT-01 pilot furniture (stage, koi_pond, window)

Derived, not redrawn, from the Codex raw PNGs `UI-024_placeable_{stage,koi_pond,window}_v01.png`
(branch `task/art-asset-production`, read-only): haze removed, cropped to the illustrated area and
resampled to 256 px per tile by `tools/art/furniture_pilot/make_pilot_assets.py`.
`koi_pond.png` is the full picture (flat base); `koi_pond_raised.png` is the same picture with the
water cut out, so it aligns pixel for pixel. Calibration lives in `IllustratedFurniture.cs`.
