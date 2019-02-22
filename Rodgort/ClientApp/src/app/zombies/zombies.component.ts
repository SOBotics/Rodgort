import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { toUtcDateTime } from '../../utils/ToUtcDateTime';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-zombies',
  templateUrl: './zombies.component.html',
  styleUrls: ['./zombies.component.scss']
})
export class ZombiesComponent implements OnInit {

  public loading: boolean;
  public zombies: any;

  public filter = {
    onlyAlive: true,
    tag: '',
  };

  constructor(
    private route: ActivatedRoute,
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filter.onlyAlive = (params['onlyAlive'] || 'true') === 'true';
      this.filter.tag = params['tag'] || '';
      this.reloadData();
    });
  }

  private reloadData() {
    this.loading = true;
    this.httpClient.get(`/api/zombie` +
      `?onlyAlive=${this.filter.onlyAlive}` +
      `&tag=${this.filter.tag}`
    ).subscribe((d: any[]) => {
      this.loading = false;
      this.zombies = d.map(zombie => ({
        ...zombie,
        lines: zombie.revivals.map(r => ({
          colour: '#ff0000',
          position: toUtcDateTime(r)
        }))
      }));
    });
  }
}
