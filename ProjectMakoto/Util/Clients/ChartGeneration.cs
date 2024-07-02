// Project Makoto
// Copyright (C) 2024  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Util;
internal class ChartGeneration(Bot bot) : RequiresBotReference(bot)
{
    internal Chart GetChart(int Width, int Height, IEnumerable<string> Labels, IEnumerable<Dataset> Datasets, int Min, int Max)
    {
        var v = $@"{{
                    type: 'line',
                    data: 
                    {{
                        labels: 
                        [
                            {string.Join(",", Labels.Select(x => $"'{x}'"))}
                        ],
                        datasets: 
                        [
                            {string.Join(",\n", Datasets.Select(x =>
                            {
                                return $@"{{
                                    label: '{x.Name}',
                                    data: [{string.Join(",", x.Data)}],
                                    fill: false,
                                    reverse: {x.Reverse.ToString().ToLower()},
                                    borderColor: {x.Color ?? "getGradientFillHelper('vertical', ['#4287f5', '#ff0000'])"},
                                    id: ""{x.Id}""
                                }}";
                            }))}
                        ]

                    }},
                    options:
                    {{
                        legend:
                        {{
                            display: true,
                        }},
                        elements:
                        {{
                            point:
                            {{
                                radius: 0
                            }}
                        }}{(Min == -1 && Max == -1 ? "" : $@"
                        ,
                        scales: {{
                            yAxes: [{{
                            ticks: {{
                                max: {Max},
                                min: {Min}
                                }}
                            }}]
                        }}
                        ")}
                    }}
                }}";

        return new(this.Bot.status.LoadedConfig.Secrets.QuickChart.Scheme, this.Bot.status.LoadedConfig.Secrets.QuickChart.Host, this.Bot.status.LoadedConfig.Secrets.QuickChart.Port)
        {
            Width = Width,
            Height = Height,
            Config = v
        };
    }

    internal record Dataset(string Name, IEnumerable<string> Data, string? Color = null, string Id = "yaxis2", bool Reverse = false);
}
