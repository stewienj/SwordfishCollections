using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetainedSelectionTest.Data
{
    /**
     *
     * A fantasy name generator library.
     * Version: 1.0.1
     * License: Public Domain
     * Author: John Stewien
     * Based On: C++ version by German M. Bravo (Kronuz)
     *
     *
     * This library is designed after the RinkWorks Fantasy Name Generator.
     * @see http://www.rinkworks.com/namegen/
     *
     * @example
     * NameGen::Generator generator("sV'i");
     * generator.toString();  // Returns a new name each call with produce()
     * // => "entheu'loaf"
     *
     * ## Pattern Syntax
     *
     *   The compile() function creates a name generator based on an input
     * pattern. The letters s, v, V, c, B, C, i, m, M, D, and d represent
     * different types of random replacements. Everything else is produced
     * literally.
     *
     *   s - generic syllable
     *   v - vowel
     *   V - vowel or vowel combination
     *   c - consonant
     *   B - consonant or consonant combination suitable for beginning a word
     *   C - consonant or consonant combination suitable anywhere in a word
     *   i - insult
     *   m - mushy name
     *   M - mushy name ending
     *   D - consonant suited for a stupid person's name
     *   d - syllable suited for a stupid person's name (begins with a vowel)
     *
     *   All characters between parenthesis () are produced literally. For
     * example, the pattern "s(dim)", produces a random generic syllable
     * followed by "dim".
     *
     *   Characters between angle brackets <> produce patterns from the table
     * above. Imagine the entire pattern is wrapped in one of these.
     *
     *   In both types of groupings, a vertical bar | denotes a random
     * choice. Empty groups are allowed. For example, "(foo|bar)" produces
     * either "foo" or "bar". The pattern "<c|v|>" produces a constant,
     * vowel, or nothing at all.
     *
     *   An exclamation point ! means to capitalize the component that
     * follows it. For example, "!(foo)" will produce "Foo" and "v!s" will
     * produce a lowercase vowel followed by a capitalized syllable, like
     * "eRod".
     *
     *   A tilde ~ means to reverse the letters of the component that
     * follows it. For example, "~(foo)" will produce "oof". To reverse an
     * entire template, wrap it in brackets. For example, to reverse
     * "sV'i" as a whole use "~<sV'i>". The template "~sV'i" will only
     * reverse the initial syllable.
     *
     * ## Internals
     *
     *   A name generator is anything with a toString() method, including,
     * importantly, strings themselves. The generator constructors
     * (Random, Sequence) perform additional optimizations when *not* used
     * with the `new` keyword: they may pass through a provided generator,
     * combine provided generators, or even return a simple string.
     *
     *   New pattern symbols added to Generator.symbols will automatically
     * be used by the compiler.
     */

/*
public class NameGen
{

    // Middle Earth
    string MIDDLE_EARTH = "(bil|bal|ban|hil|ham|hal|hol|hob|wil|me|or|ol|od|gor|for|fos|tol|ar|fin|ere|leo|vi|bi|bren|thor)(|go|orbis|apol|adur|mos|ri|i|na|ole|n)(|tur|axia|and|bo|gil|bin|bras|las|mac|grim|wise|l|lo|fo|co|ra|via|da|ne|ta|y|wen|thiel|phin|dir|dor|tor|rod|on|rdo|dis)";

    // Japanese Names (Constrained)
    string JAPANESE_NAMES_CONSTRAINED = "(aka|aki|bashi|gawa|kawa|furu|fuku|fuji|hana|hara|haru|hashi|hira|hon|hoshi|ichi|iwa|kami|kawa|ki|kita|kuchi|kuro|marui|matsu|miya|mori|moto|mura|nabe|naka|nishi|no|da|ta|o|oo|oka|saka|saki|sawa|shita|shima|i|suzu|taka|take|to|toku|toyo|ue|wa|wara|wata|yama|yoshi|kei|ko|zawa|zen|sen|ao|gin|kin|ken|shiro|zaki|yuki|asa)(||||||||||bashi|gawa|kawa|furu|fuku|fuji|hana|hara|haru|hashi|hira|hon|hoshi|chi|wa|ka|kami|kawa|ki|kita|kuchi|kuro|marui|matsu|miya|mori|moto|mura|nabe|naka|nishi|no|da|ta|o|oo|oka|saka|saki|sawa|shita|shima|suzu|taka|take|to|toku|toyo|ue|wa|wara|wata|yama|yoshi|kei|ko|zawa|zen|sen|ao|gin|kin|ken|shiro|zaki|yuki|sa)";

    // Japanese Names (Diverse)
    string JAPANESE_NAMES_DIVERSE = "(a|i|u|e|o|||||)(ka|ki|ki|ku|ku|ke|ke|ko|ko|sa|sa|sa|shi|shi|shi|su|su|se|so|ta|ta|chi|chi|tsu|te|to|na|ni|ni|nu|nu|ne|no|no|ha|hi|fu|fu|he|ho|ma|ma|ma|mi|mi|mi|mu|mu|mu|mu|me|mo|mo|mo|ya|yu|yu|yu|yo|ra|ra|ra|ri|ru|ru|ru|re|ro|ro|ro|wa|wa|wa|wa|wo|wo)(ka|ki|ki|ku|ku|ke|ke|ko|ko|sa|sa|sa|shi|shi|shi|su|su|se|so|ta|ta|chi|chi|tsu|te|to|na|ni|ni|nu|nu|ne|no|no|ha|hi|fu|fu|he|ho|ma|ma|ma|mi|mi|mi|mu|mu|mu|mu|me|mo|mo|mo|ya|yu|yu|yu|yo|ra|ra|ra|ri|ru|ru|ru|re|ro|ro|ro|wa|wa|wa|wa|wo|wo)(|(ka|ki|ki|ku|ku|ke|ke|ko|ko|sa|sa|sa|shi|shi|shi|su|su|se|so|ta|ta|chi|chi|tsu|te|to|na|ni|ni|nu|nu|ne|no|no|ha|hi|fu|fu|he|ho|ma|ma|ma|mi|mi|mi|mu|mu|mu|mu|me|mo|mo|mo|ya|yu|yu|yu|yo|ra|ra|ra|ri|ru|ru|ru|re|ro|ro|ro|wa|wa|wa|wa|wo|wo)|(ka|ki|ki|ku|ku|ke|ke|ko|ko|sa|sa|sa|shi|shi|shi|su|su|se|so|ta|ta|chi|chi|tsu|te|to|na|ni|ni|nu|nu|ne|no|no|ha|hi|fu|fu|he|ho|ma|ma|ma|mi|mi|mi|mu|mu|mu|mu|me|mo|mo|mo|ya|yu|yu|yu|yo|ra|ra|ra|ri|ru|ru|ru|re|ro|ro|ro|wa|wa|wa|wa|wo|wo)(|(ka|ki|ki|ku|ku|ke|ke|ko|ko|sa|sa|sa|shi|shi|shi|su|su|se|so|ta|ta|chi|chi|tsu|te|to|na|ni|ni|nu|nu|ne|no|no|ha|hi|fu|fu|he|ho|ma|ma|ma|mi|mi|mi|mu|mu|mu|mu|me|mo|mo|mo|ya|yu|yu|yu|yo|ra|ra|ra|ri|ru|ru|ru|re|ro|ro|ro|wa|wa|wa|wa|wo|wo)))(|||n)";

    // Chinese Names
    string CHINESE_NAMES = "(zh|x|q|sh|h)(ao|ian|uo|ou|ia)(|(l|w|c|p|b|m)(ao|ian|uo|ou|ia)(|n)|-(l|w|c|p|b|m)(ao|ian|uo|ou|ia)(|(d|j|q|l)(a|ai|iu|ao|i)))";

    // Greek Names
    string GREEK_NAMES = "<s<v|V>(tia)|s<v|V>(os)|B<v|V>c(ios)|B<v|V><c|C>v(ios|os)>";

    // Hawaiian Names (1)
    string HAWAIIAN_NAMES_1 = "((h|k|l|m|n|p|w|')|)(a|e|i|o|u)((h|k|l|m|n|p|w|')|)(a|e|i|o|u)(((h|k|l|m|n|p|w|')|)(a|e|i|o|u)|)(((h|k|l|m|n|p|w|')|)(a|e|i|o|u)|)(((h|k|l|m|n|p|w|')|)(a|e|i|o|u)|)(((h|k|l|m|n|p|w|')|)(a|e|i|o|u)|)";

    // Hawaiian Names (2)
    string HAWAIIAN_NAMES_2 = "((h|k|l|m|n|p|w|)(a|e|i|o|u|a'|e'|i'|o'|u'|ae|ai|ao|au|oi|ou|eu|ei)(k|l|m|n|p|)|)(h|k|l|m|n|p|w|)(a|e|i|o|u|a'|e'|i'|o'|u'|ae|ai|ao|au|oi|ou|eu|ei)(k|l|m|n|p|)";

    // Old Latin Place Names
    string OLD_LATIN_PLACE_NAMES = "sv(nia|lia|cia|sia)";

    // Dragons (Pern)
    string DRAGONS_PERN = "<<s|ss>|<VC|vC|B|BVs|Vs>><v|V|v|<v(l|n|r)|vc>>(th)";

    // Dragon Riders
    string DRAGON_RIDERS = "c'<s|cvc>";

    // Pokemon
    string POKEMON = "<i|s>v(mon|chu|zard|rtle)";

    // Fantasy (Vowels, R, etc.)
    string FANTASY_VOWELS_R = "(|(<B>|s|h|ty|ph|r))(i|ae|ya|ae|eu|ia|i|eo|ai|a)(lo|la|sri|da|dai|the|sty|lae|due|li|lly|ri|na|ral|sur|rith)(|(su|nu|sti|llo|ria|))(|(n|ra|p|m|lis|cal|deu|dil|suir|phos|ru|dru|rin|raap|rgue))";

    // Fantasy (S, A, etc.)
    string FANTASY_S_A = "(cham|chan|jisk|lis|frich|isk|lass|mind|sond|sund|ass|chad|lirt|und|mar|lis|il|<BVC>)(jask|ast|ista|adar|irra|im|ossa|assa|osia|ilsa|<vCv>)(|(an|ya|la|sta|sda|sya|st|nya))";

    // Fantasy (H, L, etc.)
    string FANTASY_H_L = "(ch|ch't|sh|cal|val|ell|har|shar|shal|rel|laen|ral|jh't|alr|ch|ch't|av)(|(is|al|ow|ish|ul|el|ar|iel))(aren|aeish|aith|even|adur|ulash|alith|atar|aia|erin|aera|ael|ira|iel|ahur|ishul)";

    // Fantasy (N, L, etc.)
    string FANTASY_N_L = "(ethr|qil|mal|er|eal|far|fil|fir|ing|ind|il|lam|quel|quar|quan|qar|pal|mal|yar|um|ard|enn|ey)(|(<vc>|on|us|un|ar|as|en|ir|ur|at|ol|al|an))(uard|wen|arn|on|il|ie|on|iel|rion|rian|an|ista|rion|rian|cil|mol|yon)";

    // Fantasy (K, N, etc.)
    string FANTASY_K_N = "(taith|kach|chak|kank|kjar|rak|kan|kaj|tach|rskal|kjol|jok|jor|jad|kot|kon|knir|kror|kol|tul|rhaok|rhak|krol|jan|kag|ryr)(<vc>|in|or|an|ar|och|un|mar|yk|ja|arn|ir|ros|ror)(|(mund|ard|arn|karr|chim|kos|rir|arl|kni|var|an|in|ir|a|i|as))";

    // Fantasy (J, G, Z, etc.)
    string FANTASY_J_G_Z = "(aj|ch|etz|etzl|tz|kal|gahn|kab|aj|izl|ts|jaj|lan|kach|chaj|qaq|jol|ix|az|biq|nam)(|(<vc>|aw|al|yes|il|ay|en|tom||oj|im|ol|aj|an|as))(aj|am|al|aqa|ende|elja|ich|ak|ix|in|ak|al|il|ek|ij|os|al|im)";

    // Fantasy (K, J, Y, etc.)
    string FANTASY_K_J_Y = "(yi|shu|a|be|na|chi|cha|cho|ksa|yi|shu)(th|dd|jj|sh|rr|mk|n|rk|y|jj|th)(us|ash|eni|akra|nai|ral|ect|are|el|urru|aja|al|uz|ict|arja|ichi|ural|iru|aki|esh)";

    // Fantasy (S, E, etc.)
    string FANTASY_S_E = "(syth|sith|srr|sen|yth|ssen|then|fen|ssth|kel|syn|est|bess|inth|nen|tin|cor|sv|iss|ith|sen|slar|ssil|sthen|svis|s|ss|s|ss)(|(tys|eus|yn|of|es|en|ath|elth|al|ell|ka|ith|yrrl|is|isl|yr|ast|iy))(us|yn|en|ens|ra|rg|le|en|ith|ast|zon|in|yn|ys)";


    public class Generator
    {
        protected List<Generator> generators;

        public static Dictionary<string, string[]> SymbolMap()
        {
            var symbols = new Dictionary<string, string[]>
            {
                {
                    "s", new[]
                    {
                        "ach", "ack", "ad", "age", "ald", "ale", "an", "ang", "ar", "ard",
                        "as", "ash", "at", "ath", "augh", "aw", "ban", "bel", "bur", "cer",
                        "cha", "che", "dan", "dar", "del", "den", "dra", "dyn", "ech", "eld",
                        "elm", "em", "en", "end", "eng", "enth", "er", "ess", "est", "et",
                        "gar", "gha", "hat", "hin", "hon", "ia", "ight", "ild", "im", "ina",
                        "ine", "ing", "ir", "is", "iss", "it", "kal", "kel", "kim", "kin",
                        "ler", "lor", "lye", "mor", "mos", "nal", "ny", "nys", "old", "om",
                        "on", "or", "orm", "os", "ough", "per", "pol", "qua", "que", "rad",
                        "rak", "ran", "ray", "ril", "ris", "rod", "roth", "ryn", "sam",
                        "say", "ser", "shy", "skel", "sul", "tai", "tan", "tas", "ther",
                        "tia", "tin", "ton", "tor", "tur", "um", "und", "unt", "urn", "usk",
                        "ust", "ver", "ves", "vor", "war", "wor", "yer"
                    }
                },

                {
                    "v", new[]
                    {
                        "a", "e", "i", "o", "u", "y"

                    }
                },
                {
                    "V", new[]
                    {
                        "a", "e", "i", "o", "u", "y", "ae", "ai", "au", "ay", "ea", "ee",
                        "ei", "eu", "ey", "ia", "ie", "oe", "oi", "oo", "ou", "ui"

                    }
                },
                {
                    "c", new[]
                    {
                        "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r",
                        "s", "t", "v", "w", "x", "y", "z"

                    }
                },
                {
                    "B", new[]
                    {
                        "b", "bl", "br", "c", "ch", "chr", "cl", "cr", "d", "dr", "f", "g",
                        "h", "j", "k", "l", "ll", "m", "n", "p", "ph", "qu", "r", "rh", "s",
                        "sch", "sh", "sl", "sm", "sn", "st", "str", "sw", "t", "th", "thr",
                        "tr", "v", "w", "wh", "y", "z", "zh"
                    }
                },
                {
                    "C", new[]
                    {
                        "b", "c", "ch", "ck", "d", "f", "g", "gh", "h", "k", "l", "ld", "ll",
                        "lt", "m", "n", "nd", "nn", "nt", "p", "ph", "q", "r", "rd", "rr",
                        "rt", "s", "sh", "ss", "st", "t", "th", "v", "w", "y", "z"

                    }
                },
                {
                    "i", new[]
                    {
                        "air", "ankle", "ball", "beef", "bone", "bum", "bumble", "bump",
                        "cheese", "clod", "clot", "clown", "corn", "dip", "dolt", "doof",
                        "dork", "dumb", "face", "finger", "foot", "fumble", "goof",
                        "grumble", "head", "knock", "knocker", "knuckle", "loaf", "lump",
                        "lunk", "meat", "muck", "munch", "nit", "numb", "pin", "puff",
                        "skull", "snark", "sneeze", "thimble", "twerp", "twit", "wad",
                        "wimp", "wipe"

                    }
                },
                {
                    "m", new[]
                    {
                        "baby", "booble", "bunker", "cuddle", "cuddly", "cutie", "doodle",
                        "foofie", "gooble", "honey", "kissie", "lover", "lovey", "moofie",
                        "mooglie", "moopie", "moopsie", "nookum", "poochie", "poof",
                        "poofie", "pookie", "schmoopie", "schnoogle", "schnookie",
                        "schnookum", "smooch", "smoochie", "smoosh", "snoogle", "snoogy",
                        "snookie", "snookum", "snuggy", "sweetie", "woogle", "woogy",
                        "wookie", "wookum", "wuddle", "wuddly", "wuggy", "wunny"
                    }
                },
                {
                    "M", new[]
                    {
                        "boo", "bunch", "bunny", "cake", "cakes", "cute", "darling",
                        "dumpling", "dumplings", "face", "foof", "goo", "head", "kin",
                        "kins", "lips", "love", "mush", "pie", "poo", "pooh", "pook", "pums"
                    }
                },
                {
                    "D", new[]
                    {
                        "b", "bl", "br", "cl", "d", "f", "fl", "fr", "g", "gh", "gl", "gr",
                        "h", "j", "k", "kl", "m", "n", "p", "th", "w"

                    }
                },
                {
                    "d", new[]
                    {
                        "elch", "idiot", "ob", "og", "ok", "olph", "olt", "omph", "ong",
                        "onk", "oo", "oob", "oof", "oog", "ook", "ooz", "org", "ork", "orm",
                        "oron", "ub", "uck", "ug", "ulf", "ult", "um", "umb", "ump", "umph",
                        "un", "unb", "ung", "unk", "unph", "unt", "uzz"
                    }
                }
            };
            return symbols;
        }
    }

    public enum wrappers_t
    {
        capitalizer,
        reverser
    };

    public enum group_types_t
    {
        symbol,
        literal
    };


    public class Group
    {
        private Stack<wrappers_t> wrappers;
        private List<Generator> set;

        public group_types_t type;

    }


    public class GroupSymbol : Group
    {
    }


    public class GroupLiteral : Group
    {
    }


}
*/

/* 
 * Code not converted yet from C++


size_t Generator::combinations()
{
size_t total = 1;
for (auto& g : generators) {
    total *= g->combinations();
}
return total;
}


size_t Generator::min()
{
size_t final = 0;
for (auto& g : generators) {
    final += g->min();
}
return final;
}


size_t Generator::max()
{
size_t final = 0;
for (auto& g : generators) {
    final += g->max();
}
return final;
}


std::string Generator::toString() {
std::string str;
for (auto& g : generators) {
    str.append(g->toString());
}
return str;
}


void Generator::add(std::unique_ptr<Generator>&& g)
{
generators.push_back(std::move(g));
}



size_t Random::combinations()
{
size_t total = 0;
for (auto& g : generators) {
    total += g->combinations();
}
return total ? total : 1;
}

size_t Random::min()
{
size_t final = -1;
for (auto& g : generators) {
    size_t current = g->min();
    if (current < final) {
        final = current;
    }
}
return final;
}

size_t Random::max()
{
size_t final = 0;
for (auto& g : generators) {
    size_t current = g->max();
    if (current > final) {
        final = current;
    }
}
return final;
}


std::string Random::toString()
{
if (!generators.size()) {
    return "";
}
std::uniform_real_distribution<double> distribution(0, generators.size() - 1);
int rnd = distribution(rng) + 0.5;
return generators[rnd]->toString();
}


Literal::Literal(const std::string &value_) :
value(value_)
{
}

size_t Literal::combinations()
{
return 1;
}

size_t Literal::min()
{
return value.size();
}

size_t Literal::max()
{
return value.size();
}

std::string Literal::toString()
{
return value;
}

Reverser::Reverser(std::unique_ptr<Generator>&& g)
{
add(std::move(g));
}


std::string Reverser::toString()
{
std::wstring str = towstring(Generator::toString());
std::reverse(str.begin(), str.end());
return tostring(str);
}

Capitalizer::Capitalizer(std::unique_ptr<Generator>&& g)
{
add(std::move(g));
}

std::string Capitalizer::toString()
{
std::wstring str = towstring(Generator::toString());
str[0] = std::towupper(str[0]);
return tostring(str);
}


Collapser::Collapser(std::unique_ptr<Generator>&& g)
{
add(std::move(g));
}

std::string Collapser::toString()
{
std::wstring str = towstring(Generator::toString());
std::wstring out;
int cnt = 0;
wchar_t pch = L'\0';
for (auto ch : str) {
    if (ch == pch) {
        cnt++;
    } else {
        cnt = 0;
    }
    int mch = 2;
    switch(ch) {
        case 'a':
        case 'h':
        case 'i':
        case 'j':
        case 'q':
        case 'u':
        case 'v':
        case 'w':
        case 'x':
        case 'y':
            mch = 1;
    }
    if (cnt < mch) {
        out.push_back(ch);
    }
    pch = ch;
}
return tostring(out);
}


Generator::Generator(const std::string &pattern, bool collapse_triples) {
std::unique_ptr<Generator> last;

std::stack<std::unique_ptr<Group>> stack;
std::unique_ptr<Group> top = make_unique<GroupSymbol>();

for (auto c : pattern) {
    switch (c) {
        case '<':
            stack.push(std::move(top));
            top = make_unique<GroupSymbol>();
            break;
        case '(':
            stack.push(std::move(top));
            top = make_unique<GroupLiteral>();
            break;
        case '>':
        case ')':
            if (stack.size() == 0) {
                throw std::invalid_argument("Unbalanced brackets");
            } else if (c == '>' && top->type != group_types::symbol) {
                throw std::invalid_argument("Unexpected '>' in pattern");
            } else if (c == ')' && top->type != group_types::literal) {
                throw std::invalid_argument("Unexpected ')' in pattern");
            }
                            last = top->produce();
            top = std::move(stack.top());
            stack.pop();
            top->add(std::move(last));
            break;
        case '|':
            top->split();
            break;
        case '!':
            if (top->type == group_types::symbol) {
                top->wrap(wrappers::capitalizer);
            } else {
                top->add(c);
            }
            break;
        case '~':
            if (top->type == group_types::symbol) {
                top->wrap(wrappers::reverser);
            } else {
                top->add(c);
            }
            break;
        default:
            top->add(c);
            break;
    }
}

if (stack.size() != 0) {
    throw std::invalid_argument("Missing closing bracket");
}

    std::unique_ptr<Generator> g = top->produce();
if (collapse_triples) {
    g = make_unique<Collapser>(std::move(g));
}
add(std::move(g));
}


Generator::Group::Group(group_types_t type_) :
type(type_)
{
}

void Generator::Group::add(std::unique_ptr<Generator>&& g)
{
while (!wrappers.empty()) {
    switch (wrappers.top()) {
        case reverser:
            g = make_unique<Reverser>(std::move(g));
            break;
        case capitalizer:
            g = make_unique<Capitalizer>(std::move(g));
            break;
    }
    wrappers.pop();
}
if (set.size() == 0) {
    set.push_back(make_unique<Sequence>());
}
set.back()->add(std::move(g));
}

void Generator::Group::add(char c)
{
std::string value(1, c);
std::unique_ptr<Generator> g = make_unique<Random>();
g->add(make_unique<Literal>(value));
Group::add(std::move(g));
}

std::unique_ptr<Generator> Generator::Group::produce()
{
switch (set.size()) {
    case 0:
        return make_unique<Literal>("");
    case 1:
        return std::move(*set.begin());
    default:
        return make_unique<Random>(std::move(set));
}
}

void Generator::Group::split()
{
if (set.size() == 0) {
    set.push_back(make_unique<Sequence>());
}
set.push_back(make_unique<Sequence>());
}

void Generator::Group::wrap(wrappers_t type)
{
wrappers.push(type);
}

Generator::GroupSymbol::GroupSymbol() :
Group(group_types::symbol)
{
}

void Generator::GroupSymbol::add(char c)
{
std::string value(1, c);
std::unique_ptr<Generator> g = make_unique<Random>();
try {
    static const auto& symbols = SymbolMap();
    for (const auto& s : symbols.at(value)) {
        g->add(make_unique<Literal>(s));
    }
} catch (const std::out_of_range&) {
    g->add(make_unique<Literal>(value));
}
Group::add(std::move(g));
}

Generator::GroupLiteral::GroupLiteral() :
Group(group_types::literal)
{
}

std::wstring towstring(const std::string & s)
{
const char *cs = s.c_str();
const size_t wn = std::mbsrtowcs(nullptr, &cs, 0, nullptr);

if (wn == static_cast<size_t>(-1)) {
    return L"";
}

std::vector<wchar_t> buf(wn);
cs = s.c_str();
const size_t wn_again = std::mbsrtowcs(buf.data(), &cs, wn, nullptr);

if (wn_again == static_cast<size_t>(-1)) {
    return L"";
}

return std::wstring(buf.data(), wn);
}

std::string tostring(const std::wstring & s)
{
const wchar_t *cs = s.c_str();
const size_t wn = std::wcsrtombs(nullptr, &cs, 0, nullptr);

if (wn == static_cast<size_t>(-1)) {
    return "";
}

std::vector<char> buf(wn);
const size_t wn_again = std::wcsrtombs(buf.data(), &cs, wn, nullptr);

if (wn_again == static_cast<size_t>(-1)) {
    return "";
}

return std::string(buf.data(), wn);
}
*/
}
