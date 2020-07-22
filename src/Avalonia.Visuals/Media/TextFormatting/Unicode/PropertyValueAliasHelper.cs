using System.Collections.Generic;

namespace Avalonia.Media.TextFormatting.Unicode
{
    internal static class PropertyValueAliasHelper
    {
        private static readonly Dictionary<Script, string> s_scriptToTag = 
            new Dictionary<Script, string>{
                { Script.Unknown, "Zzzz"},
                { Script.Common, "Zyyy"},
                { Script.Inherited, "Zinh"},
                { Script.Adlam, "Adlm"},
                { Script.CaucasianAlbanian, "Aghb"},
                { Script.Ahom, "Ahom"},
                { Script.Arabic, "Arab"},
                { Script.ImperialAramaic, "Armi"},
                { Script.Armenian, "Armn"},
                { Script.Avestan, "Avst"},
                { Script.Balinese, "Bali"},
                { Script.Bamum, "Bamu"},
                { Script.BassaVah, "Bass"},
                { Script.Batak, "Batk"},
                { Script.Bengali, "Beng"},
                { Script.Bhaiksuki, "Bhks"},
                { Script.Bopomofo, "Bopo"},
                { Script.Brahmi, "Brah"},
                { Script.Braille, "Brai"},
                { Script.Buginese, "Bugi"},
                { Script.Buhid, "Buhd"},
                { Script.Chakma, "Cakm"},
                { Script.CanadianAboriginal, "Cans"},
                { Script.Carian, "Cari"},
                { Script.Cham, "Cham"},
                { Script.Cherokee, "Cher"},
                { Script.Chorasmian, "Chrs"},
                { Script.Coptic, "Copt"},
                { Script.Cypriot, "Cprt"},
                { Script.Cyrillic, "Cyrl"},
                { Script.Devanagari, "Deva"},
                { Script.DivesAkuru, "Diak"},
                { Script.Dogra, "Dogr"},
                { Script.Deseret, "Dsrt"},
                { Script.Duployan, "Dupl"},
                { Script.EgyptianHieroglyphs, "Egyp"},
                { Script.Elbasan, "Elba"},
                { Script.Elymaic, "Elym"},
                { Script.Ethiopic, "Ethi"},
                { Script.Georgian, "Geor"},
                { Script.Glagolitic, "Glag"},
                { Script.GunjalaGondi, "Gong"},
                { Script.MasaramGondi, "Gonm"},
                { Script.Gothic, "Goth"},
                { Script.Grantha, "Gran"},
                { Script.Greek, "Grek"},
                { Script.Gujarati, "Gujr"},
                { Script.Gurmukhi, "Guru"},
                { Script.Hangul, "Hang"},
                { Script.Han, "Hani"},
                { Script.Hanunoo, "Hano"},
                { Script.Hatran, "Hatr"},
                { Script.Hebrew, "Hebr"},
                { Script.Hiragana, "Hira"},
                { Script.AnatolianHieroglyphs, "Hluw"},
                { Script.PahawhHmong, "Hmng"},
                { Script.NyiakengPuachueHmong, "Hmnp"},
                { Script.KatakanaOrHiragana, "Hrkt"},
                { Script.OldHungarian, "Hung"},
                { Script.OldItalic, "Ital"},
                { Script.Javanese, "Java"},
                { Script.KayahLi, "Kali"},
                { Script.Katakana, "Kana"},
                { Script.Kharoshthi, "Khar"},
                { Script.Khmer, "Khmr"},
                { Script.Khojki, "Khoj"},
                { Script.KhitanSmallScript, "Kits"},
                { Script.Kannada, "Knda"},
                { Script.Kaithi, "Kthi"},
                { Script.TaiTham, "Lana"},
                { Script.Lao, "Laoo"},
                { Script.Latin, "Latn"},
                { Script.Lepcha, "Lepc"},
                { Script.Limbu, "Limb"},
                { Script.LinearA, "Lina"},
                { Script.LinearB, "Linb"},
                { Script.Lisu, "Lisu"},
                { Script.Lycian, "Lyci"},
                { Script.Lydian, "Lydi"},
                { Script.Mahajani, "Mahj"},
                { Script.Makasar, "Maka"},
                { Script.Mandaic, "Mand"},
                { Script.Manichaean, "Mani"},
                { Script.Marchen, "Marc"},
                { Script.Medefaidrin, "Medf"},
                { Script.MendeKikakui, "Mend"},
                { Script.MeroiticCursive, "Merc"},
                { Script.MeroiticHieroglyphs, "Mero"},
                { Script.Malayalam, "Mlym"},
                { Script.Modi, "Modi"},
                { Script.Mongolian, "Mong"},
                { Script.Mro, "Mroo"},
                { Script.MeeteiMayek, "Mtei"},
                { Script.Multani, "Mult"},
                { Script.Myanmar, "Mymr"},
                { Script.Nandinagari, "Nand"},
                { Script.OldNorthArabian, "Narb"},
                { Script.Nabataean, "Nbat"},
                { Script.Newa, "Newa"},
                { Script.Nko, "Nkoo"},
                { Script.Nushu, "Nshu"},
                { Script.Ogham, "Ogam"},
                { Script.OlChiki, "Olck"},
                { Script.OldTurkic, "Orkh"},
                { Script.Oriya, "Orya"},
                { Script.Osage, "Osge"},
                { Script.Osmanya, "Osma"},
                { Script.Palmyrene, "Palm"},
                { Script.PauCinHau, "Pauc"},
                { Script.OldPermic, "Perm"},
                { Script.PhagsPa, "Phag"},
                { Script.InscriptionalPahlavi, "Phli"},
                { Script.PsalterPahlavi, "Phlp"},
                { Script.Phoenician, "Phnx"},
                { Script.Miao, "Plrd"},
                { Script.InscriptionalParthian, "Prti"},
                { Script.Rejang, "Rjng"},
                { Script.HanifiRohingya, "Rohg"},
                { Script.Runic, "Runr"},
                { Script.Samaritan, "Samr"},
                { Script.OldSouthArabian, "Sarb"},
                { Script.Saurashtra, "Saur"},
                { Script.SignWriting, "Sgnw"},
                { Script.Shavian, "Shaw"},
                { Script.Sharada, "Shrd"},
                { Script.Siddham, "Sidd"},
                { Script.Khudawadi, "Sind"},
                { Script.Sinhala, "Sinh"},
                { Script.Sogdian, "Sogd"},
                { Script.OldSogdian, "Sogo"},
                { Script.SoraSompeng, "Sora"},
                { Script.Soyombo, "Soyo"},
                { Script.Sundanese, "Sund"},
                { Script.SylotiNagri, "Sylo"},
                { Script.Syriac, "Syrc"},
                { Script.Tagbanwa, "Tagb"},
                { Script.Takri, "Takr"},
                { Script.TaiLe, "Tale"},
                { Script.NewTaiLue, "Talu"},
                { Script.Tamil, "Taml"},
                { Script.Tangut, "Tang"},
                { Script.TaiViet, "Tavt"},
                { Script.Telugu, "Telu"},
                { Script.Tifinagh, "Tfng"},
                { Script.Tagalog, "Tglg"},
                { Script.Thaana, "Thaa"},
                { Script.Thai, "Thai"},
                { Script.Tibetan, "Tibt"},
                { Script.Tirhuta, "Tirh"},
                { Script.Ugaritic, "Ugar"},
                { Script.Vai, "Vaii"},
                { Script.WarangCiti, "Wara"},
                { Script.Wancho, "Wcho"},
                { Script.OldPersian, "Xpeo"},
                { Script.Cuneiform, "Xsux"},
                { Script.Yezidi, "Yezi"},
                { Script.Yi, "Yiii"},
                { Script.ZanabazarSquare, "Zanb"},
        };

        public static string GetTag(Script script)
        {
            if(!s_scriptToTag.ContainsKey(script))
            {
                return "Zzzz";
            }
            return s_scriptToTag[script];
        }
    }
}
