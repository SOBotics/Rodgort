import { Component, OnInit, Input } from '@angular/core';
import { Chart } from 'angular-highcharts';
import * as Highcharts from 'highcharts';

@Component({
  selector: 'app-question-count-graph',
  templateUrl: './question-count-graph.component.html',
  styleUrls: ['./question-count-graph.component.scss']
})
export class QuestionCountGraphComponent implements OnInit {

  @Input()
  public data: any;
  @Input()
  public featuredStarted?: string;
  @Input()
  public featuredEnded?: string;
  @Input()
  public burnStarted?: string;
  @Input()
  public burnEnded?: string;

  public chart: Chart = null;

  constructor() { }

  ngOnInit() {
    const bands = [];
    const lines = [];
    if (this.featuredStarted && this.featuredEnded) {
      bands.push({
        color: 'red',
        from: this.toUtcDateTime(this.featuredStarted),
        to: this.toUtcDateTime(this.featuredEnded),
        label: {
          text: 'featured'
        }
      });
    } else if (this.featuredStarted) {
      lines.push({
        color: 'red',
        value: this.toUtcDateTime(this.featuredStarted),
        width: 2,
        label: {
          text: 'featured start'
        }
      });
    } else if (this.featuredEnded) {
      lines.push({
        color: 'red',
        value: this.toUtcDateTime(this.featuredEnded),
        width: 2,
        label: {
          text: 'featured end'
        }
      });
    }

    if (this.burnStarted && this.burnEnded) {
      bands.push({
        color: 'red',
        from: this.toUtcDateTime(this.burnStarted),
        to: this.toUtcDateTime(this.burnEnded),
        label: {
          text: 'burnination'
        }
      });
    } else if (this.burnStarted) {
      lines.push({
        color: 'red',
        value: this.toUtcDateTime(this.burnStarted),
        width: 2,
        label: {
          text: 'burn start'
        }
      });
    } else if (this.burnEnded) {
      lines.push({
        color: 'red',
        value: this.toUtcDateTime(this.burnEnded),
        width: 2,
        label: {
          text: 'burn end'
        }
      });
    }

    const series: { name: string, data: [number, number][] }[] = [];
    series.push({
      name: '',
      data: this.data.map(gd => {
        const utcDate = this.toUtcDateTime(gd.dateTime);
        return [utcDate, gd.questionCount];
      })
    });
    this.chart = new Chart({
      chart: {
        type: 'line',
      },
      title: {
        text: ''
      },
      xAxis: {
        type: 'datetime',
        labels: {
          format: '{value:%Y-%m-%d}'
        },
        plotLines: lines,
        plotBands: bands,
      },
      tooltip: {
        formatter: function () {
          return `${this.y} questions seen on ${Highcharts.dateFormat('%Y-%m-%d %H:%M', this.x)}`;
        }
      },
      yAxis: {
        title: {
          text: 'Total questions'
        },
        min: 0
      },
      credits: {
        enabled: false
      },
      legend: {
        enabled: false
      },
      series: series
    });
  }

  private toUtcDateTime(num: string): number {
    const date = new Date(num);
    const utcDate = Date.UTC(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      date.getHours(),
      date.getMinutes(),
      date.getSeconds()
    );
    return utcDate;
  }
}
