// <copyright company="SIX Networks GmbH" file="SmileyParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SN.withSIX.Play.Core.Connect.Parser
{
    public class SmileyParser
    {
        static readonly IDictionary<Regex, string[]> smileyData = new Dictionary<Regex, string[]> {
            {new Regex(@"\:\-?\)", RegexOptions.Compiled), new[] {":)", "emoticon-00100-smile"}},
            {new Regex(@"\:\-?\(", RegexOptions.Compiled), new[] {":(", "emoticon-00101-sadsmile"}}, {
                new Regex(@"\:\-?D", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new[] {":D", "emoticon-00102-bigsmile"}
            },
            {new Regex(@"8\-?\)", RegexOptions.Compiled), new[] {"8)", "emoticon-00103-cool"}},
            {new Regex(@"\;\-?\)", RegexOptions.Compiled), new[] {";)", "emoticon-00105-wink"}},
            {new Regex(@"\;\-?\(", RegexOptions.Compiled), new[] {";(", "emoticon-00106-crying"}},
            {new Regex(@"\(sweat\)|\(\:\|", RegexOptions.Compiled), new[] {"(sweat)", "emoticon-00107-sweating"}},
            {new Regex(@"\:\||\:\=\||\:\-\|", RegexOptions.Compiled), new[] {":|", "emoticon-00108-speechless"}},
            {new Regex(@"\:(\-\=)?\*", RegexOptions.Compiled), new[] {":*", "emoticon-00109-kiss"}},
            {new Regex(@"\:\-?P", RegexOptions.Compiled), new[] {":P", "emoticon-00110-tongueout"}}, {
                new Regex(@"\(blush\)|\:\$|\:\-\$|\:\=\$", RegexOptions.Compiled),
                new[] {"(blush)", "emoticon-00111-blush"}
            },
            {new Regex(@"\:\^\)", RegexOptions.Compiled), new[] {":^)", "emoticon-00112-wondering"}}, {
                new Regex(@"\|\-\)|I\-\)|I\+\)|\(snooze\)", RegexOptions.Compiled),
                new[] {"(snooze)", "emoticon-00113-sleepy"}
            },
            {new Regex(@"\|\(|\|\-\(|\|\=\(", RegexOptions.Compiled), new[] {"|(", "emoticon-00114-dull"}},
            {new Regex(@"\(involve\)", RegexOptions.Compiled), new[] {"(inlove)", "emoticon-00115-inlove"}},
            {new Regex(@"\}\:\)|\>\:\)|\(grin\)", RegexOptions.Compiled), new[] {"(grin)", "emoticon-00116-evilgrin"}},
            {new Regex(@"\(talk\)", RegexOptions.Compiled), new[] {"(talk)", "emoticon-00117-talking"}},
            {new Regex(@"\(yawn\)|\|\-\(\)", RegexOptions.Compiled), new[] {"(yawn)", "emoticon-00118-yawn"}},
            {new Regex(@"\(puke\)|\:\&|\:\-\&|\:\=\&", RegexOptions.Compiled), new[] {"(puke)", "emoticon-00119-puke"}},
            {new Regex(@"\(doh\)", RegexOptions.Compiled), new[] {"(doh)", "emoticon-00120-doh"}}, {
                new Regex(@"\:\@|\:\-\@|\:\=\@|x\(|x\-\(|x\=\(", RegexOptions.Compiled),
                new[] {":@", "emoticon-00121-angry"}
            },
            {new Regex(@"\(wasntme\)", RegexOptions.Compiled), new[] {"(wasntme)", "emoticon-00122-itwasntme"}},
            {new Regex(@"\(party\)", RegexOptions.Compiled), new[] {"(party)", "emoticon-00123-party"}},
            {new Regex(@"\:\-?S|\:\=S", RegexOptions.Compiled), new[] {":S", "emoticon-00124-worried"}},
            {new Regex(@"\(mm\)", RegexOptions.Compiled), new[] {"(mm)", "emoticon-00125-mmm"}}, {
                new Regex(@"8\-?\||B\-?\||8\=\||B\=\||\(nerd\)", RegexOptions.Compiled),
                new[] {"(nerd)", "emoticon-00126-nerd"}
            }, {
                new Regex(@"\:\-?x|\:\=x|\:\-?\#|\:\=\#", RegexOptions.Compiled),
                new[] {":X", "emoticon-00127-lipssealed"}
            },
            {new Regex(@"\(hi\)", RegexOptions.Compiled), new[] {"(hi)", "emoticon-00128-hi"}},
            {new Regex(@"\(call\)", RegexOptions.Compiled), new[] {"(call)", "emoticon-00129-call"}},
            {new Regex(@"\(devil\)", RegexOptions.Compiled), new[] {"(devil)", "emoticon-00130-devil"}},
            {new Regex(@"\(angel\)", RegexOptions.Compiled), new[] {"(angel)", "emoticon-00131-angel"}},
            {new Regex(@"\(envy\)", RegexOptions.Compiled), new[] {"(envy)", "emoticon-00132-envy"}},
            {new Regex(@"\(wait\)", RegexOptions.Compiled), new[] {"(wait)", "emoticon-00133-wait"}},
            {new Regex(@"\(bear\)|\(hug\)", RegexOptions.Compiled), new[] {"(bear)", "emoticon-00134-bear"}},
            {new Regex(@"\(makeup\)|\(kate\)", RegexOptions.Compiled), new[] {"(makeup)", "emoticon-00135-makeup"}},
            {new Regex(@"\(giggle\)|\(chuckle\)", RegexOptions.Compiled), new[] {"(giggle)", "emoticon-00136-giggle"}},
            {new Regex(@"\(clap\)", RegexOptions.Compiled), new[] {"(clap)", "emoticon-00137-clapping"}}, {
                new Regex(@"\(think\)|\:\-?\?|\:\=\?", RegexOptions.Compiled),
                new[] {"(think)", "emoticon-00138-thinking"}
            },
            {new Regex(@"\(bow\)", RegexOptions.Compiled), new[] {"(bow)", "emoticon-00139-bow"}},
            {new Regex(@"\(rofl\)", RegexOptions.Compiled), new[] {"(rofl)", "emoticon-00140-rofl"}},
            {new Regex(@"\(whew\)", RegexOptions.Compiled), new[] {"(whew)", "emoticon-00141-whew"}},
            {new Regex(@"\(happy\)", RegexOptions.Compiled), new[] {"(happy)", "emoticon-00142-happy"}},
            {new Regex(@"\(smirk\)", RegexOptions.Compiled), new[] {"(smirk)", "emoticon-00143-smirk"}},
            {new Regex(@"\(nod\)", RegexOptions.Compiled), new[] {"(nod)", "emoticon-00144-nod"}},
            {new Regex(@"\(shake\)", RegexOptions.Compiled), new[] {"(shake)", "emoticon-00145-shake"}},
            {new Regex(@"\(punch\)", RegexOptions.Compiled), new[] {"(punch)", "emoticon-00146-punch"}},
            {new Regex(@"\(emo\)", RegexOptions.Compiled), new[] {"(emo)", "emoticon-00147-emo"}},
            {new Regex(@"\(y\)|\(ok\)", RegexOptions.Compiled), new[] {"(Y)", "emoticon-00148-yes"}},
            {new Regex(@"\(n\)", RegexOptions.Compiled), new[] {"(N)", "emoticon-00149-no"}},
            {new Regex(@"\(handshake\)", RegexOptions.Compiled), new[] {"(handshake)", "emoticon-00150-handshake"}},
            {new Regex(@"\(h\)|\(l\)", RegexOptions.Compiled), new[] {"(H)", "emoticon-00152-heart"}},
            {new Regex(@"\(u\)", RegexOptions.Compiled), new[] {"(U)", "emoticon-00153-brokenheart"}},
            {new Regex(@"\(e\)|\(m\)", RegexOptions.Compiled), new[] {"(e)", "emoticon-00154-mail"}},
            {new Regex(@"\(f\)", RegexOptions.Compiled), new[] {"(F)", "emoticon-00155-flower"}},
            {new Regex(@"\(rain\)", RegexOptions.Compiled), new[] {"(rain)", "emoticon-00156-rain"}},
            {new Regex(@"\(sun\)", RegexOptions.Compiled), new[] {"(sun)", "emoticon-00157-sun"}},
            {new Regex(@"\(o\)|\(time\)", RegexOptions.Compiled), new[] {"(time)", "emoticon-00158-time"}},
            {new Regex(@"\(music\)", RegexOptions.Compiled), new[] {"(music)", "emoticon-00159-music"}},
            {new Regex(@"\(\~\)|\(film\)|\(movie\)", RegexOptions.Compiled), new[] {"(movie)", "emoticon-00160-movie"}},
            {new Regex(@"\(mp\)|\(ph\)", RegexOptions.Compiled), new[] {"(ph)", "emoticon-00161-phone"}},
            {new Regex(@"\(coffee\)", RegexOptions.Compiled), new[] {"(coffee)", "emoticon-00162-coffee"}},
            {new Regex(@"\(pizza\)|\(pi\)", RegexOptions.Compiled), new[] {"(pizza)", "emoticon-00163-pizza"}},
            {new Regex(@"\(cash\)|\(mo\)|\(\$\)", RegexOptions.Compiled), new[] {"(cash)", "emoticon-00164-cash"}},
            {new Regex(@"\(muscle\)|\(flex\)", RegexOptions.Compiled), new[] {"(muscle)", "emoticon-00165-muscle"}},
            {new Regex(@"\(\^\)|\(cake\)", RegexOptions.Compiled), new[] {"(cake)", "emoticon-00166-cake"}},
            {new Regex(@"\(beer\)", RegexOptions.Compiled), new[] {"(beer)", "emoticon-00167-beer"}},
            {new Regex(@"\(d\)", RegexOptions.Compiled), new[] {"(d)", "emoticon-00168-drink"}},
            {new Regex(@"\(dance\)|\\o\/|\\\:D\/", RegexOptions.Compiled), new[] {"(dance)", "emoticon-00169-dance"}},
            {new Regex(@"\(ninja\)", RegexOptions.Compiled), new[] {"(ninja)", "emoticon-00170-ninja"}},
            {new Regex(@"\(\*\)", RegexOptions.Compiled), new[] {"(*)", "emoticon-00171-star"}},
            {new Regex(@"\(mooning\)", RegexOptions.Compiled), new[] {"(mooning)", "emoticon-00172-mooning"}},
            {new Regex(@"\(finger\)", RegexOptions.Compiled), new[] {"(finger)", "emoticon-00173-middlefinger"}},
            {new Regex(@"\(bandit\)", RegexOptions.Compiled), new[] {"(bandit)", "emoticon-00174-bandit"}},
            {new Regex(@"\(drunk\)", RegexOptions.Compiled), new[] {"(drunk)", "emoticon-00175-drunk"}}, {
                new Regex(@"\(smoking\)|\(smoke\)|\(ci\)", RegexOptions.Compiled),
                new[] {"(smoking)", "emoticon-00176-smoke"}
            },
            {new Regex(@"\(toivo\)", RegexOptions.Compiled), new[] {"(toivo)", "emoticon-00177-toivo"}},
            {new Regex(@"\(rock\)", RegexOptions.Compiled), new[] {"(rock)", "emoticon-00178-rock"}}, {
                new Regex(@"\(headbang\)|\(banghead\)", RegexOptions.Compiled),
                new[] {"(headbang)", "emoticon-00179-headbang"}
            },
            {new Regex(@"\(bug\)", RegexOptions.Compiled), new[] {"(bug)", "emoticon-00180-bug"}},
            {new Regex(@"\(fubar\)", RegexOptions.Compiled), new[] {"(fubar)", "emoticon-00181-fubar"}},
            {new Regex(@"\(poolparty\)", RegexOptions.Compiled), new[] {"(poolparty)", "emoticon-00182-poolparty"}},
            {new Regex(@"\(swear\)", RegexOptions.Compiled), new[] {"(swear)", "emoticon-00183-swear"}},
            {new Regex(@"\(tmi\)", RegexOptions.Compiled), new[] {"(tmi)", "emoticon-00184-tmi"}},
            {new Regex(@"\(heidy\)", RegexOptions.Compiled), new[] {"(heidy)", "emoticon-00185-heidy"}},
            {new Regex(@"\(pilot\)|\:pilot", RegexOptions.Compiled), new[] {"(pilot)", "pilotfly"}}
        };
        static readonly string baseURL = "https://d2l9k1uqfxdpqe.cloudfront.net/assets/smileys/";
        static readonly string extension = ".gif";

        public static IEnumerable<ChatTokenDto> Parse(string text) {
            var results = new List<ChatTokenDto>();

            foreach (var smiley in smileyData) {
                var matches = smiley.Key.Matches(text);

                if (matches.Count > 0) {
                    foreach (Match m in matches) {
                        results.Add(new ChatTokenDto {
                            StartIndex = m.Index,
                            EndIndex = m.Index + m.Length - 1,
                            Content = baseURL + smiley.Value[1] + extension,
                            TokenType = ChatTokenType.Image
                        });
                    }
                }
            }

            return results;
        }
    }
}