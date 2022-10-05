using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms.Design;

namespace Smart.FormDesigner.Services
{
    /// <inheritdoc/>
    public class DefaultDesignerOptionService : DesignerOptionService
    {
        protected override void PopulateOptionCollection(DesignerOptionCollection options)
        {
            if (options.Parent == null)
            {
                base.CreateOptionCollection(options, "DesignerOptions", new DesignerOptions()
                {
                    GridSize = new Size(8, 8),
                    ShowGrid = false,
                    UseSmartTags = true,
                    UseSnapLines = true,
                    ObjectBoundSmartTagAutoShow = true,
                    EnableInSituEditing = true,
                    SnapToGrid = true,
                    UseOptimizedCodeGeneration = false,
                });
            }
        }
    }
}
