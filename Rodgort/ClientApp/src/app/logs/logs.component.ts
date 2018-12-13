import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PagingInfo, GetPagingInfo } from '../../utils/PagingHelper';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-logs',
  templateUrl: './logs.component.html',
  styleUrls: ['./logs.component.scss']
})
export class LogsComponent implements OnInit {

  public pagingInfo: PagingInfo[];
  public filter = {
    pageNumber: 1,
  };

  public logs: any[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filter.pageNumber = +params['pageNumber'] || 1;
      this.reloadData();
    });
  }


  public reloadData() {
    const query =
      `/api/logs` +
      `?page=${this.filter.pageNumber}`;

    this.httpClient.get(query).subscribe((response: any) => {
      if (response.totalPages > 0 && response.pageNumber > response.totalPages) {
        this.filter.pageNumber = 1;
        this.reloadData();
      } else {
        console.log(response);
        this.logs = response.data;
        this.pagingInfo = GetPagingInfo(response);
      }
    });
  }

  public loadPage(pageNumber: number) {
    this.filter.pageNumber = pageNumber;
    this.applyFilter();
  }

  public applyFilter() {
    this.router.navigate(['/logs'], { queryParams: this.filter });
  }
}
