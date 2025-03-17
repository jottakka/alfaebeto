using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordProcessing.Util;

public sealed record PickRightOptionFromHintData(
    string ToBeGuessed,
    int AnswerIdx,
    string[] ShuffledOptions
    );
