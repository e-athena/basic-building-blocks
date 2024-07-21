using JiebaNet.Segmenter;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace Athena.Infrastructure.Lucene.Analyzers;

public class JieBaTokenizer : Tokenizer
{
    private string _inputText;

    private ICharTermAttribute _termAtt;
    private IOffsetAttribute _offsetAtt;
    private IPositionIncrementAttribute _posIncrAtt;
    private ITypeAttribute _typeAtt;
    private readonly Dictionary<string, int> _stopWords = new();
    private readonly List<JiebaNet.Segmenter.Token> _wordList = [];


    private IEnumerator<JiebaNet.Segmenter.Token> _iter;
    private readonly JiebaSegmenter _segmenter;
    private readonly TokenizerMode _mode;

    /// <summary>
    ///
    /// </summary>
    /// <param name="input"></param>
    /// <param name="mode"></param>
    public JieBaTokenizer(TextReader input, TokenizerMode mode)
        : base(AttributeFactory.DEFAULT_ATTRIBUTE_FACTORY, input)
    {
        _segmenter = new JiebaSegmenter();
        _mode = mode;
        Init();
    }

    /// <summary>
    /// 加载停用词
    /// </summary>
    /// <param name="filePath"></param>
    private void LoadStopWords(string filePath)
    {
        using (var reader = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + filePath))
        {
            string tmp;
            while ((tmp = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(tmp))
                {
                    continue;
                }

                if (_stopWords.ContainsKey(tmp))
                {
                    continue;
                }

                _stopWords.Add(tmp, 1);
            }
        }
    }

    /// <summary>
    /// 初始化（添加属性）
    /// </summary>
    private void Init()
    {
        _termAtt = AddAttribute<ICharTermAttribute>();
        _offsetAtt = AddAttribute<IOffsetAttribute>();
        _posIncrAtt = AddAttribute<IPositionIncrementAttribute>();
        _typeAtt = AddAttribute<ITypeAttribute>();
    }

    private string ReadToEnd(TextReader input)
    {
        return input.ReadToEnd();
    }

    public sealed override Boolean IncrementToken()
    {
        ClearAttributes();

        var token = Next();
        if (token != null)
        {
            var buffer = token.ToString();
            _termAtt.SetEmpty().Append(buffer);
            _offsetAtt.SetOffset(CorrectOffset(token.StartOffset), CorrectOffset(token.EndOffset));
            _typeAtt.Type = token.Type;
            return true;
        }

        End();
        this.Dispose();
        return false;
    }

    public global::Lucene.Net.Analysis.Token? Next()
    {
        var res = _iter.MoveNext();
        if (res)
        {
            var current = _iter.Current;
            if (current != null)
            {
                var token =
                    new global::Lucene.Net.Analysis.Token(current.Word, current.StartIndex, current.EndIndex);
                return token;
            }

            return null;
        }

        return null;
    }

    public override void Reset()
    {
        base.Reset();
        _inputText = ReadToEnd(m_input);
        IEnumerable<JiebaNet.Segmenter.Token> tokens = _segmenter.Tokenize(_inputText, _mode); //获取JieBa分词Token
        _wordList.Clear(); //清除分词列表
        foreach (var token in tokens)
        {
            _wordList.Add(token);
        }

        _iter = _wordList.GetEnumerator();
    }
}