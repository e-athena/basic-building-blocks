using JiebaNet.Segmenter;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.TokenAttributes;

namespace Athena.Infrastructure.Lucene.Analyzers;

public class JieBaAnalyzer : Analyzer
{
    private readonly TokenizerMode _mode;

    /// <summary>
    ///
    /// </summary>
    /// <param name="mode"></param>
    public JieBaAnalyzer(TokenizerMode mode) : base()
    {
        _mode = mode;
    }

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        var tokenizer = new JieBaTokenizer(reader, _mode);

        var tokenStream =
            (TokenStream)new LowerCaseFilter(global::Lucene.Net.Util.LuceneVersion.LUCENE_48, tokenizer);

        tokenStream.AddAttribute<ICharTermAttribute>();
        tokenStream.AddAttribute<IOffsetAttribute>();

        return new TokenStreamComponents(tokenizer, tokenStream);
    }
}