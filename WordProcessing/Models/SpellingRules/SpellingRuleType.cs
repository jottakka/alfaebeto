namespace WordProcessing.Models.SpellingRules;
public enum SpellingRuleRuleType
{
    // Rule types for SOrZ
    SemRegraS,          // Sem regra específica (S)
    SufixosOrigem,      // Sufixos -ês, -esa, -ense, -isa
    PrefixoDes,         // Prefixo des-
    SufixosOso,         // Sufixos -oso, -osa, -ase, -ese, -isa, -ose
    VerbosPorQuerer,    // Conjugação dos verbos pôr e querer
    VerbosComS,         // Verbos e derivados com S
    SemRegraZ,          // Sem regra específica (Z)
    ExcecoesRegraS,     // Exceções para regras de S
    SufixosIzar,        // Sufixos -izar
    SufixosEzEza,       // Substantivos abstratos com -ez, -eza

    // Rule types for XorCH
    // Rule types for XorCH
    SemRegraX,          // Sem regra específica (X)
    DitongoX,           // X após ditongos
    OrigemIndigenaAfricanaInglesa, // X em palavras de origem indígena, africana ou inglesa
    EnX,                // X após a sílaba inicial 'en-'
    MeX,                // X após a sílaba '-me'
    SemRegraCH,         // Sem regra específica (CH)
    EstrangeirismoCH,   // CH em palavras de origem estrangeira
    MechaCH,            // CH em 'mecha'
    VerboEncherCH       // CH no verbo encher
}
