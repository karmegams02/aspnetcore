// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Rendering;

namespace Microsoft.AspNetCore.Components.QuickGrid;

/// <summary>
/// A lightweight, runtime-configurable column definition that can be rendered by <see cref="QuickGrid{TGridItem}"/>.
/// Use <see cref="DynamicColumn{TGridItem}"/> when the columns are not known at compile time (for example, when they
/// are supplied dynamically from data, configuration, or user input). A dynamic column specifies either a
/// <see cref="Property"/> expression to project a value or a <see cref="Template"/> fragment to render arbitrary
/// content, and the surrounding <c>QuickGrid</c> materializes the appropriate <see cref="PropertyColumn{TGridItem, TProp}"/>
/// or <see cref="TemplateColumn{TGridItem}"/> based on which member is set.
/// </summary>
/// <typeparam name="TGridItem">The type of data represented by each row in the grid.</typeparam>
public partial class DynamicColumn<TGridItem> : ColumnBase<TGridItem>
{
    /// <summary>
    /// Gets or sets a lambda expression selecting the value to be displayed in this column's cells.
    /// When set, the column is rendered as a <see cref="PropertyColumn{TGridItem, TProp}"/> projecting this expression.
    /// Mutually exclusive with <see cref="Template"/>; exactly one of the two must be supplied.
    /// </summary>
    [Parameter]
    public Expression<Func<TGridItem, object?>>? Property { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column is currently rendered by the grid.
    /// Defaults to <see langword="true"/>. Toggle via <see cref="QuickGrid{TGridItem}.HideColumnAsync(string)"/>
    /// and <see cref="QuickGrid{TGridItem}.ShowColumnAsync(string)"/> to show or hide a column at runtime.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <inheritdoc />
    public override GridSort<TGridItem>? SortBy { get; set; }

    /// <inheritdoc />
    protected internal override void CellContent(RenderTreeBuilder builder, TGridItem item)
    {
        // DynamicColumn itself is never rendered directly. QuickGrid inspects the configured
        // Property or Template and materializes a concrete PropertyColumn or TemplateColumn,
        // so this method should never be invoked. The throw guards against unintended direct use.
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets or sets a <see cref="RenderFragment"/> used to render the cell content of this column.
    /// When set, the column is rendered as a <see cref="TemplateColumn{TGridItem}"/> using this template.
    /// Mutually exclusive with <see cref="Property"/>; exactly one of the two must be supplied.
    /// </summary>
    public RenderFragment? Template { get; set; }
}

