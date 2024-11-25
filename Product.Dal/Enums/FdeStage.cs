using System.ComponentModel;

namespace Product.Dal.Enums
{
    public enum FdeStage
    {
        [Description("Submitted")]
        Submitted,
        [Description("FdeReceived")]
        FdeReceived,
        [Description("Normalize")]
        Normalize,
        [Description("Ocr")]
        Ocr,
        [Description("DocExtract")]
        DocExtract,
        [Description("OcrToCsv")]
        OcrToCsv,
        [Description("PredictIs")]
        PredictIs,
        [Description("PredictBs")]
        PredictBs,
        [Description("Adjustments")]
        Adjustments,
        [Description("OutputIs")]
        OutputIs,
        [Description("OutputBs")]
        OutputBs,
        [Description("Completed")]
        Completed
    }
}
