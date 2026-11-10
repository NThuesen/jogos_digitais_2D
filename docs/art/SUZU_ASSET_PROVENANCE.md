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
f705ce15120717a2ad5c21a7125ee6c76bb173853ea90d92841269f7a15d6508  Assets/_Game/Resources/Furniture/table_2.png
702c87197a6bc3386bc721e90c6d037050881b3223c2fc5d3e2cffb109a6256e  Assets/_Game/Resources/Furniture/table_2_front.png
18f90f9f137d9a953baa0a3178ea1499ec796cbef6706c1a2618ea369c3c29b8  Assets/_Game/Resources/Furniture/table_2_rotated.png
62b1a085d956eeefa843bbf737b5f378479775cd016387ac7621509dedaa68e1  Assets/_Game/Resources/Furniture/table_4.png
668f6239875569a4f0b852ccb4f741fc17e12ffaebebd1c497cf28a99356f5db  Assets/_Game/Resources/Furniture/table_4_front.png
dcf6b400ea299d6f41083b2876411eb9f6a18924a973f11a97e4bf4320a65944  Assets/_Game/Resources/Furniture/stool.png
355ad76b219a51298bfd82ebceae2ac4dc54a461e20f69f1a260b9b88f8d5117  Assets/_Game/Resources/Furniture/stove.png
c9f025cf7bacf7bcdf7f97218add106847c22c73f7d64c7a270a65b2fc752e1f  Assets/_Game/Resources/Furniture/teabar.png
5689be2f61c4f1da4f6fb25eb20d1d768f7dc992d68645b902cf83a72ab55531  Assets/_Game/Resources/Furniture/dessert.png
76e0ef3a15f6e35d2d63b00377cd136697a8a4b1ccbbfedd7f44349a0d1764d2  Assets/_Game/Resources/Furniture/bakery.png
```

## ART-INT-01 pilot furniture (stage, koi_pond, window)

Derived, not redrawn, from the Codex raw PNGs `UI-024_placeable_{stage,koi_pond,window}_v01.png`
(branch `task/art-asset-production`, read-only): haze removed, cropped to the illustrated area and
resampled to 256 px per tile by `tools/art/furniture_pilot/make_pilot_assets.py`.
`koi_pond.png` is the full picture (flat base); `koi_pond_raised.png` is the same picture with the
water cut out, so it aligns pixel for pixel. Calibration lives in `IllustratedFurniture.cs`.

Source files as read (read-only, `neko-art-assets/art_working/ui_modular/raw_gpt/`; SHA-256 written
`file: hash` so the allowlist parser above does not treat them as imported files):

- `UI-024_placeable_stage_v01.png`: ae502d78df3e711701b3e2c107cdb4941e0e135ede95491a3be026cd78b33a57
- `UI-024_placeable_koi_pond_v01.png`: 1e6d641d1d740d52f23d7888d057cde510380b1df32e5bca074bd8a8f1e0726b
- `UI-024_placeable_window_v01.png`: bd3f08b450800839f96c217b14eaf0adabbb9fa09b101c94af917a9f84832a95

## UI-INT-01 kit (Hub and Caixinhas)

Copied byte for byte (no resampling) from the delivery `neko-ui-kit-delivery/` (`sprites/*_v01.png`, `CLEAN/hub_background_v01.png`, `CLEAN/caixinhas_background_v01.png`) by `tools/art/ui_kit/import_ui_kit.py`; 9-slice borders come from the kit manifest and are in each .meta and in `Resources/UiKit/SOURCES.json`. All are RGBA with real transparency, and none carries gameplay text.

e5ef58ad0f1f859a887d965f7bd975c69505573cb33b4adb4e53fd3bb5b2a0b3  Assets/_Game/Resources/UiKit/chest_simple.png
d1f15c839aecc436f668bb3f9038216da8e384d047964c7ace0d79636a0dbdb5  Assets/_Game/Resources/UiKit/chest_silver.png
5c6b33e8e8205c26db3803579bce76cd391a0944f87019670ced2b1384551340  Assets/_Game/Resources/UiKit/chest_gold.png
77bd749bdea99b9a12eaf175eab0a85938814d3bfb66a9511b89f0ca200a4c75  Assets/_Game/Resources/UiKit/box_simple_frame.png
593ad638cc2a56e850bf57b72253aceed8757c69e5c4b52cd5ebd834cc816c5f  Assets/_Game/Resources/UiKit/box_silver_frame.png
ca527a4b9472739dfc6d56b99da3bccbb5a12dd041c7dfd972bd79db551c599e  Assets/_Game/Resources/UiKit/box_gold_frame.png
d3403a9e09fbdb926106fdd80ade01e33ae392627eb49873af90526b4ec87c30  Assets/_Game/Resources/UiKit/economy_frame.png
f8dec965255c7fc4d91c8cea1dd1de163f371dc1e8afd8497acb9d645633ccb4  Assets/_Game/Resources/UiKit/level_frame.png
fd835a80de132066715d878062ed0bd84305d9c2d22bb7d019d289c6737b06ae  Assets/_Game/Resources/UiKit/bow_gold.png
324a4d765b244d3bc41b727ec8da27d5b5259a575bb4694b96c2f2d1a24147fc  Assets/_Game/Resources/UiKit/bow_blue.png
55bdac395033c218e6f6ed555bc20431753308265e8da326d175fe39f55e4926  Assets/_Game/Resources/UiKit/bow_cta.png
aaf9871a4268115651c2c856b56bdf9ab109545973bc267fd186560c5b2cb8c5  Assets/_Game/Resources/UiKit/cta_blank.png
19221bbd094bda039468c1963928f3f4cffd6a7047beb8bdc0a60bb0cbd0f060  Assets/_Game/Resources/UiKit/nav_pink_blank.png
7cf05cd1a18895405108e1472c10181feffbd1eadf082cdbc5059efa7931927e  Assets/_Game/Resources/UiKit/nav_active_blank.png
226056fd20785f719f0cfa8c02803a83097e6bd17f2ca614668f4fcff987320f  Assets/_Game/Resources/UiKit/box_button_active_blank.png
33a9a00d8c35acc05795f777e5e2048247eab56f8030fa9763ab45c74bdf2eec  Assets/_Game/Resources/UiKit/box_button_brown_blank.png
857aff026587f9d8578af596fed2d3f5b518e9420303b88f185b1e480d5a19e1  Assets/_Game/Resources/UiKit/back_blank.png
f89384db21f3004bc8cbbca34aa5bcc9944970fe57d5e06f9b8cea8b4d879493  Assets/_Game/Resources/UiKit/title_blank.png
d1cfc05c2d4e1449a24e4e70a78fee2faaba228a67037c05085e6b92448ad04c  Assets/_Game/Resources/UiKit/warning_blank.png
72ae0e794149fa252b0018a03f55e5286b742934db99490e05ef27f0a632f08e  Assets/_Game/Resources/UiKit/box_simple_panel_empty.png
a0c2483ef13f4f78b4c8062ddd3e9980ad9b8aff1b6a8b58454fc3612b2e2509  Assets/_Game/Resources/UiKit/box_silver_panel_empty.png
42d76edb43888f6c75c8631573282c46470138c41ea5542bb937d5ada2bf0b1f  Assets/_Game/Resources/UiKit/box_gold_panel_empty.png
e0f4e4c5f13595f8e6a1ab8c9f54b008cc06715b317896ab928b93b9de176583  Assets/_Game/Resources/UiKit/paw_cream.png
25594b0b713a559217e13a39377db44df516060672801259b2dcc13afaaec1c1  Assets/_Game/Resources/UiKit/paw_gold.png
f707d0549f7b0847a12127b7113caa83005f7a8b5b244bd202ab2442e24682c4  Assets/_Game/Resources/UiKit/level_track_empty.png
f2e75b2b588ee7364695c5216e35d1abea37f2f2b064e4b5bd6c3aeabc725138  Assets/_Game/Resources/UiKit/level_fill.png
83dd51e807c6e9e0e38ecf03ca4de8caf44b0bcd370196d25ac2b3488a6ecc2f  Assets/_Game/Resources/UiKit/economy_panel_empty.png
eb86fa5234503bc6e02d1fb5635151f8bed709e35a7bc12ac3e7c093307b718f  Assets/_Game/Resources/UiKit/level_panel_empty.png
d57ec9dec45c5daf6c6537c5229709d10d7c7cf199d6c71a85c5c54fd4d12d5f  Assets/_Game/Resources/UiKit/nav_pink_surface.png
4d71d87efda5ceed6373a33c7eef4199ddc84edacf81f8055d91920123cbfecb  Assets/_Game/Resources/UiKit/box_name_frame.png
f3d6c13a6d07f4a9c3cb25fcf3a63d9454f053ea115125da6a4cba82e2f81add  Assets/_Game/Resources/UiKit/box_name_empty.png
603842c49c5e0fa26aa07e9c2f778695631b559aff492d905d7e94a89ac59ce0  Assets/_Game/Resources/UiKit/odds_frame.png
0e82358104a790b42ce279541e90ae21a2d4f4e2a8b3c29b3d303c5a52c680a7  Assets/_Game/Resources/UiKit/odds_panel_empty.png
795d5e0c2399f4317dfb7f61ca7ecd56e2bdd5bba4cf95baa03fce4898e80c40  Assets/_Game/Resources/UiKit/hub_background.png
657f21d15c928652391cbe6292acac788fb631e8d5d0d8600854aacc4cfa987f  Assets/_Game/Resources/UiKit/caixinhas_background.png

## ART-INT-02 functional furniture (table_2, table_2_rotated, table_4, stool, stove, teabar, dessert, bakery)

Derived, not redrawn, from the ART-PROD-02C CLEAN exports `UI-024_placeable_<id>_clean_c01.png`
(delivery `art-prod-02c-delivery/`, branch `task/art-prod-02`, read-only; every source SHA-256
checked against its `INVENTORY.json` before use): cropped to the illustrated area and resampled with
premultiplied alpha to 256 px per tile by `tools/art/furniture_functional/make_functional_assets.py`
(calibration in `calibration.json`, matching `IllustratedFurniture.cs`). `table_2_front.png` and
`table_4_front.png` are the same pictures with the parts that sit behind a seated customer made
transparent (chairs behind the top), so they align pixel for pixel. Art status: ART_ACCEPTED by the owner;
no TECH_PASS is claimed.

Source SHA-256 (source file, hash):

| Source | SHA-256 |
|---|---|
| `UI-024_placeable_table_2_v02_clean_c01.png` | `689f266fe395ec0c387374ef3e95dafedf8b4f2693d38eaa64603f66283eba03` |
| `UI-024_placeable_table_2_rotated_v02_clean_c01.png` | `f507ae0ddf4a2b7ee1b340b2d5e9cbc3ff8aa7efc6bc6f94986166a1169d3c6f` |
| `UI-024_placeable_table_4_v03_clean_c01.png` | `aa715267980d9b9874353e6699cae7a94bef58061a6dea2ae928b32f7ce0d5df` |
| `UI-024_placeable_stool_v01_clean_c01.png` | `e72bcffde34dfcbac5314af1fc606c0362f9a0ef803eed1583baf7986aeaac70` |
| `UI-024_placeable_stove_v01_clean_c01.png` | `4b86ad6b146ae34ddd982fd14c0cb834b269d845a3cf8101089ff7bcdb0110bb` |
| `UI-024_placeable_teabar_v01_clean_c01.png` | `ac024b9365d6f2c1698a46ed46abfdfd2737f4609cfae16802d15bfd8c2a580f` |
| `UI-024_placeable_dessert_v01_clean_c01.png` | `c73774f918faad121d6c9c598c347300c18d395a3ad5dfa47ef8b43f9f018603` |
| `UI-024_placeable_bakery_v01_clean_c01.png` | `80cef4abf11bba490d805a08df2d74da761f853e3ecd7887b23123b7a13005ee` |

## UI-INT-02A kit (Maids / Elenco)

Copied byte for byte (no resampling) from the delivery `neko-ui-maids-kit-delivery/` (`sprites/*_v01.png`, `backgrounds/maids_background_v01.png`) by `tools/art/ui_kit/import_maids_kit.py`, prefixed `maids_`; 9-slice borders come from the kit manifest and are in each .meta and in `Resources/UiKit/SOURCES.json`. All are RGBA with real transparency and none carries gameplay text. The four resources the kit reuses from UI-VISUAL-01 (`back_blank`, `bow_gold`, `paw_gold`, `paw_cream`) are the files already in UiKit. Not imported: the equip and level-up plates (no such action exists), the `*_frame` alternates and the brown surfaces (the panels are used whole).

f7855dc43639a5f82b7572ca41d704c5452cf43f5e0d841211198b0db2d6febe  Assets/_Game/Resources/UiKit/maids_background.png
479f61e087b25c5bb45cb2fbd5eb339415aa87afbf9d346534cfd1230fca04b5  Assets/_Game/Resources/UiKit/maids_title_plate_empty.png
558f336aa567ba1d4378ff6b6246df6e2249b05cd9094e614d5ec627c1cee9f5  Assets/_Game/Resources/UiKit/maids_token_counter_empty.png
f3f5257f58369d79bd0882e5059cf5a4292197068310ed7afd4aee0f6d6b63c7  Assets/_Game/Resources/UiKit/maids_filters_container_empty.png
ccfa4c614278069c00504223873ff90fc639d0010cd312c196fa9621eb67b8bd  Assets/_Game/Resources/UiKit/maids_filter_row_divider.png
a4e621ed3ede230f34d65c5ee288a204049ffe32ec6cf25f50fbeaae24546d36  Assets/_Game/Resources/UiKit/maids_filter_selected_empty.png
06bbc1797b19a2d6edf19b6ef83ac651aa1eceb5739d3fa3e79f1d2ecd44f254  Assets/_Game/Resources/UiKit/maids_filter_selected_surface.png
752e37ac8e1f94ea260c5106415145bc644c5d8e74a18aa8ffcdd2006641a45f  Assets/_Game/Resources/UiKit/maids_filter_rarity_normal_empty.png
34a37b7639f227e2d783911c79437c74c67f6c2f7031a63c73c854ce5e884bdd  Assets/_Game/Resources/UiKit/maids_filter_position_normal_empty.png
3d36466fadc6d2267817b98b2b2f8b800d62e66c0af668a46b86b66d653af1e0  Assets/_Game/Resources/UiKit/maids_filter_paw_brown.png
ae4308b5d1931e71cfacd61129299b4649ed0794487e084822ad70109fef6d71  Assets/_Game/Resources/UiKit/maids_icon_reception.png
35883ad2cd1b0face28ca688a04bd48f5459877ac0b390a02661f7e9d3023851  Assets/_Game/Resources/UiKit/maids_icon_hall.png
9cedeac88b55d0527d3490fa4c0b6d93d728013fa6ce4592954710e2c84a71c7  Assets/_Game/Resources/UiKit/maids_icon_kitchen.png
81440f372bc867295543e867cc302a4bbb463ddc1714c8a32e6491ffd65dde72  Assets/_Game/Resources/UiKit/maids_sort_empty.png
58e634e50336da30d5dec243ee4bb98516a9edd0e236a1dcb6181270cdeeec47  Assets/_Game/Resources/UiKit/maids_sort_divider.png
1d9e68db3a46c9abaf805f5b5f13e93733835d673230cfd4c7f3682b74d7380c  Assets/_Game/Resources/UiKit/maids_collection_panel_empty.png
2e539749625f8e4e50f3239939bac664f1f8ecbed6b2981345ed6a1154e215ae  Assets/_Game/Resources/UiKit/maids_selected_card_highlight.png
2de4e80f4181c4aa66d2327aa4c58a5bfd4c8f7f667be301ca33c40644b38345  Assets/_Game/Resources/UiKit/maids_page_left.png
f59f6b489ae2d25fbb99f2f34fffeffc24a34d3ff6c42b97edc0753d2b55dea8  Assets/_Game/Resources/UiKit/maids_page_right.png
db9ccab8f1c8e9497757f41aa231f70aa5136b62b33363e8a7e19bff719c5bbe  Assets/_Game/Resources/UiKit/maids_page_plate_empty.png
9dc744c44463b57afbb1565b177d7ea1d0fc3c0e1ace4f449936a2d956113618  Assets/_Game/Resources/UiKit/maids_detail_panel_empty.png
9038f71976ca82063689caf0e0efc7f0c37b60f1547d6ddc92a4a59f78266578  Assets/_Game/Resources/UiKit/maids_portrait_frame_empty.png
1098d569c59919819b65ee327d0169eac1a8d3a3939d9900a7d8d642967d9368  Assets/_Game/Resources/UiKit/maids_rarity_badge_empty.png
c29fbdbff037ebf955b1f366f113e7d546030518be5b2ca8dffd153c72b84c94  Assets/_Game/Resources/UiKit/maids_attribute_track_empty.png
ade8acde7a000a6fe6037425d342f2e3837d775ff7985c02e120a3fd165d4389  Assets/_Game/Resources/UiKit/maids_attribute_fill_gold.png
90d68362cb3055a8468bd65a2b73b91b554e1b4d5a66fd2b56bafa9be1f4e847  Assets/_Game/Resources/UiKit/maids_traits_frame_empty.png
e506d5189137c91cd69ee762b47680a6218633f4bd43a27529838d68ffc7962a  Assets/_Game/Resources/UiKit/maids_dismiss_empty.png

## UI-INT-03B kits (Equipe do Dia / Lojinha)

Copied byte for byte (no resampling) from the deliveries `neko-ui-lineup-kit-delivery/` and `neko-ui-shop-kit-delivery/` by `tools/art/ui_kit/import_lineup_shop_kits.py`, prefixed `lineup_` / `shop_`; 9-slice borders come from each kit manifest and are in each .meta and in `Resources/UiKit/SOURCES.json`. All are RGBA and none carries gameplay text. Resources the kits reuse from earlier work (`back_blank`, `bow_gold`, `paw_gold`, `paw_cream`, `maids_token_counter_empty`, `icon_coin`, the logo) are the files already in the project. Not imported: `*_frame`/`*_surface` alternates, `*_icon_window` rows, the clipboard alone, the ready-made vacant slot and the outer chemistry frame (each panel is used whole).

643b0f35ac07d08e46e9f7a20ec21d6d0c182e807fa9fd7dbc8edb8a76d740a7  Assets/_Game/Resources/UiKit/lineup_role_plate_empty.png
36a6f517697231ed7c3010fde69ae1c37e8ce413d685e36b7fcc2157f5363035  Assets/_Game/Resources/UiKit/lineup_vacant_slot_frame.png
d92b98477316daa2487ca2398286c2e55a0ac0c369e59af2b07a52ef0e1ea39f  Assets/_Game/Resources/UiKit/lineup_vacant_paw_plus.png
e16e8b366df887adc69027c361854ce845a36ca69b81425c9edad157842752e9  Assets/_Game/Resources/UiKit/lineup_chemistry_value_panel_empty.png
2056e063fbeab951de4ed1e56822c18482c7b5ac32291bcfba937969fd9b4d37  Assets/_Game/Resources/UiKit/lineup_daily_cost_panel_empty.png
802fcc2e09870d76d56109b3deaa941b63f4d48bdcedf4e5d10d901383f31042  Assets/_Game/Resources/UiKit/lineup_chemistry_header_empty.png
d1cb7afa635394d0974ca75a9c9d0ef5602b8952b8df71d40c1ac2c45e2d591b  Assets/_Game/Resources/UiKit/lineup_chemistry_track_empty.png
fc9b851279ceb729042b3e8de5eb8ef64eff9c12522dd26ef1e67d0ee6063a3c  Assets/_Game/Resources/UiKit/lineup_chemistry_fill_pink.png
cb0aee000d27194c8167c2657c253812047dae90d018d360b5dfb8d9fb286a84  Assets/_Game/Resources/UiKit/lineup_title_empty.png
cec05d38cd0a1d2562201884a60f4141b053991e1210b5d21b9aa1fbb456459b  Assets/_Game/Resources/UiKit/lineup_team_banner_empty.png
d23623fa216939ebbcf4dda84d8be87518ae1ccd849c36ed52b801569cacf414  Assets/_Game/Resources/UiKit/lineup_bank_banner_empty.png
0ab10a2a1d23e4f02ae08d0c71ee6a7fb7f68ef1fdef1cac9fcb6cdccf342fd7  Assets/_Game/Resources/UiKit/lineup_bank_filter_empty.png
5364cfbd4f1763086615f0bd8f97f0b3d045cd90e591e3042f9ae4048f9b831a  Assets/_Game/Resources/UiKit/lineup_bank_sort_empty.png
c9de57f7ca997437d765f3edce09be368d86c1e9dd4c338f5ff1c0a80aaab31d  Assets/_Game/Resources/UiKit/lineup_bank_page_empty.png
31e381ddf9f8030eab37b3f6a15023733b75c317609567feb9eebae73c233ef0  Assets/_Game/Resources/UiKit/lineup_bank_left.png
5e9515c46a868a55037e24f3c9b507cdb8215f2f8d4a3f346ed97fc6087e01ee  Assets/_Game/Resources/UiKit/lineup_bank_right.png
ed0aec13cba8cdd5c9bccf2b602c59d5ac71dc07317432e4900b01b71085aa97  Assets/_Game/Resources/UiKit/lineup_team_panel_empty.png
d9df8d77938a57f1542cb6a3d54a82147e9d5886fd98785b06801cd8b18726ac  Assets/_Game/Resources/UiKit/lineup_bank_panel_empty.png
1931aebab5cddf70a90997c7ca3c0f525a1bdd917590c8dd394585eba95cbe96  Assets/_Game/Resources/UiKit/lineup_chemistry_panel_empty.png
d7dbd499505cedde30868626df4d96870f1f15da3616896a9f36b09da1ce7ab0  Assets/_Game/Resources/UiKit/lineup_button_auto_empty.png
d9ff9638ffeb1a9b5fc650157737d9393430ab57e77f234b2aebce6ceb5ca813  Assets/_Game/Resources/UiKit/lineup_button_clear_empty.png
c80e89c3e98c70437816329d1dfd4f68263979aebeb5374e18f373561252876b  Assets/_Game/Resources/UiKit/lineup_background.png
223ee5d4b0f968749cb44e7ce39eeca3b0fb3c5bb98ba477aa3fa70c2b3482f3  Assets/_Game/Resources/UiKit/shop_tab_normal_empty.png
cdbf93bd98ae974fcd849cb1b19763a4c2a2691865a69f1ec7a6e99927fc8aa8  Assets/_Game/Resources/UiKit/shop_tab_selected_empty.png
6e86f4ae6b939d65aa8d8d33d606e71e474510dfe77d92a6682cc95f56589e56  Assets/_Game/Resources/UiKit/shop_tab_underline.png
0847c72d06d81b6ad00ebde51c28565603e03b88afd29924a1bb7e6e26e86470  Assets/_Game/Resources/UiKit/shop_buy_empty.png
36ff90b9a823290cc254756c64e03a1cddcf7d67dbc27e7096223b78bec94f04  Assets/_Game/Resources/UiKit/shop_money_panel_empty.png
dd7b1ad5525423023b197fe0c7c824a68e5c8bf0a183d927b28d769dd6be528a  Assets/_Game/Resources/UiKit/shop_title_empty.png
cb4bd1312879cc256a27bc6004c1d00e4085dc9fcc9d65ce5625b0f6be97f824  Assets/_Game/Resources/UiKit/shop_bottom_paw_ornament.png
287dadc2310d302337856dd26c7c441d6dc43439a1c2aef950a70ae4c7ca3fde  Assets/_Game/Resources/UiKit/shop_panel_top_bow.png
d6d22a234631419165e13374e6f1a4a9e2cc7d22985051cbcdaa496190978344  Assets/_Game/Resources/UiKit/shop_product_row_normal.png
d68de6f7785bffe4f3927a8d3ecc5f5393c169079e7482db41fd5cfef81e3fca  Assets/_Game/Resources/UiKit/shop_product_row_highlight.png
08111ca4afc11841c8735173381240bf7bc87ace8f17ce6ff7eda1f3dd6b5307  Assets/_Game/Resources/UiKit/shop_main_panel_empty.png
263d146ba3d64d9b7820d2380d571259cbcf2e4e9e872761c01567746be3f1ea  Assets/_Game/Resources/UiKit/shop_background.png

## UI-INT-05/06 kits (Salão / Expediente / Resultado)

Copied byte for byte (no resampling) from the deliveries `neko-ui-hall-kit-delivery/` (UI-VISUAL-05) and `neko-ui-shift-results-kit-delivery/` (UI-VISUAL-06) by `tools/art/ui_kit/import_hall_shift_results_kits.py`, prefixed `hall_` / `shift_` / `results_`; 9-slice borders come from each kit manifest and are in each .meta and in `Resources/UiKit/SOURCES.json`. All are RGBA and none carries gameplay text, values or a screenshot of the room. Resources the kits reuse (`back_blank`, `bow_gold`, `paw_cream`) are the files already in the project. Not imported: the tray frame/surface and chip frame/surface alternates, the report frame/surface alternates, the neutral (non-open) shift background, the neutral viewport support (the camera clears to the board's paper colour instead), the loose room corners, the portrait ring (the chip shows name, position, state and mood, no portrait), the unselected speed plate (the empty top HUD already carries it), and `hall_shop_access_empty` (the Hall has no Loja callback, so no button is faked).
5b8775eb0b22b0970bbbe7df9bb65defc2bd9ca9d28c8aa71fab554b94bdd323  Assets/_Game/Resources/UiKit/hall_office_background.png
552a54bebe29a8f88b5af26a44081d4ee8a15a1e8c081b64c642027fcc35ff47  Assets/_Game/Resources/UiKit/hall_board_frame.png
541d0bac57e5e7cad5607561d5db6eae7146b28ba7fcff4d17043511a9ab0783  Assets/_Game/Resources/UiKit/hall_title_empty.png
0b8bf8a0d494d95767886309a3b97cc4e9aaf1dddd8e498baded602c2541045c  Assets/_Game/Resources/UiKit/hall_metrics_plate_empty.png
172bbdb31de06af5b153ec90ac2c06322758a40c3f1c0ec358868cb012a51154  Assets/_Game/Resources/UiKit/hall_metrics_divider.png
9ff41ce38443ed1b6cc63dae8a64b7e116d3dc1a8d0056c1d1139554b79f0f17  Assets/_Game/Resources/UiKit/hall_icon_seat.png
7472eb05af73e7784e4d9f3484ff02e12b2ac1235622fb1216859e823b2cb98d  Assets/_Game/Resources/UiKit/hall_icon_ambience.png
6dee50ffdbfb1b26d39c8000ef4f4bb42be2e306e61f28e99ed5e621c6a086c4  Assets/_Game/Resources/UiKit/hall_icon_flow.png
75e5a3ad53540276f3b0d5b330ed6bb864c5e6de80f48ecdaa227c47545bb007  Assets/_Game/Resources/UiKit/hall_icon_theme.png
900bd65ee9b50f5cc942b801229f289736aa3ffd67f95e9321c2a612e1d1e812  Assets/_Game/Resources/UiKit/hall_instruction_strip_empty.png
ac316ffa89438bca80d31cfad6daf4aea6efd84d3981200a0b525db113a3d2a8  Assets/_Game/Resources/UiKit/hall_inventory_tray_empty.png
1feec55e3e1f17e1f0df70e102b30f37ebba8b9829cdf37c9d94e75c195a0e96  Assets/_Game/Resources/UiKit/hall_inventory_cell_empty.png
c71480b1eec1064332706de8b2dab6172ab9cf282fdaf738af80a2cb862e258b  Assets/_Game/Resources/UiKit/hall_inventory_cell_selected.png
4de9829a284ba004a3329d8ae90c6bfb18c2b87a361b11359c63f499500b1eab  Assets/_Game/Resources/UiKit/hall_inventory_paw.png
0e84e57df376b18da3461b28c557bb15f21bd6a504b1d4ec373c7b4197cdbb56  Assets/_Game/Resources/UiKit/hall_stock_label_empty.png
d65aaa71d123d14dba63929a8839cb2219ff3c4de31a204b1b618e947a121399  Assets/_Game/Resources/UiKit/shift_restaurant_background_open.png
5db4ecf4c1cdabeeaad48a4c274fb15dc40c3e88f418a927123d4c8225603dd2  Assets/_Game/Resources/UiKit/shift_room_frame.png
e1bbe6bd29616b00a32c0cd5bec92933c7cba6338a6bf6d462c546a02597be5d  Assets/_Game/Resources/UiKit/shift_top_hud_empty.png
371d312e34303069af197739390142025dd0ea2e4d8acabed61d9ef31fe52507  Assets/_Game/Resources/UiKit/shift_maid_chip_empty.png
79f01eb39ed8e85975b73fe9ba56040ee92a51a679260f0f38d742078216a759  Assets/_Game/Resources/UiKit/shift_mood_track.png
2aaf0358117dcaeab27e56e3e761d10c5902115d294aaea728d0f01803668806  Assets/_Game/Resources/UiKit/shift_mood_fill.png
10ccd9cf31a545353affa2347f980b60c653a1393cedfcfde3f74545cc5c9163  Assets/_Game/Resources/UiKit/shift_speed_selected.png
f1c1ada3f9b4fd2bcc930c180fdde8c76196a994e7e078256bea8e5e4b19da85  Assets/_Game/Resources/UiKit/shift_instruction_empty.png
b7c19d292ba69f678817d3d705a0eda2b2aecdb757ed6848f6260d0cfcc7f2d3  Assets/_Game/Resources/UiKit/results_restaurant_background.png
48485049f990127a895e81043693d764908a0514a524c8f2955d0ffb8f656aca  Assets/_Game/Resources/UiKit/results_report_empty.png
31164c09440a8d8aa9bdacd33116a4270b581ca6f82dfc43dc3db62c8d035010  Assets/_Game/Resources/UiKit/results_column_empty.png
bdba81b1798b0b4c273974f53238a513fac1887825101783e4067450505da261  Assets/_Game/Resources/UiKit/results_header_empty.png
84ed884fcf013bbbc2ff4cf568300fdb2bf62adf5b4ac921d74c19a9e31a9776  Assets/_Game/Resources/UiKit/results_total_empty.png
e0bc5e71fc85c582a6a3c894629dbeeff7870bee60e604e981a59818c8529bfb  Assets/_Game/Resources/UiKit/results_tally_empty.png
18be9be0b66844a075250eaf3b7cb2be268cf324d215fdd2dd5cf63aaf3435d9  Assets/_Game/Resources/UiKit/results_financial_highlight_empty.png
e989c6763bc845907666a39b996d74d5d4e8491312a1c41fb5a15edc27105ecc  Assets/_Game/Resources/UiKit/results_continue_empty.png
a76aa079a44faa4029a486620fc8d19e64b113994a77bb2d5c84cd0ea1a4c6c8  Assets/_Game/Resources/UiKit/results_row_empty.png
378974a38a0ff6f952c313fde2908af30256eda6995a7aee2e99e173baf035f4  Assets/_Game/Resources/UiKit/results_separator.png
