import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GetPagingInfo } from '../../utils/PagingHelper';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit {

  public users: any;
  public loading = true;
  public filter = {
    userName: '',
    pageNumber: 1,
    sortBy: 'burnActions'
  };
  public pagingInfo: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpClient: HttpClient) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filter.pageNumber = +params['pageNumber'] || 1;
      this.filter.userName = params['userName'] || '';
      this.filter.sortBy = params['sortBy'] || 'burnActions';
      this.reloadData();
    });
  }

  private reloadData() {
    this.loading = true;
    const query =
      `/api/users/all` +
      `?userName=${this.filter.userName}` +
      `&sortBy=${this.filter.sortBy}` +
      `&pageNumber=${this.filter.pageNumber}`;

    this.httpClient.get(query).subscribe((response: any) => {
      this.loading = false;
      if (response.totalPages > 0 && response.pageNumber > response.totalPages) {
        this.filter.pageNumber = 1;
        this.reloadData();
      } else {
        this.users = response.data;
        this.pagingInfo = GetPagingInfo(response);
      }

      this.users = response.data;
    });
  }

  public loadPage(pageNumber: number) {
    this.filter.pageNumber = pageNumber;
    this.applyFilter();
  }

  public applyFilter() {
    this.router.navigate(['/users'], { queryParams: this.filter });
  }
}
