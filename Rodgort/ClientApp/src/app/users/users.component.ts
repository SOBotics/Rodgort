import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GetPagingInfo } from '../../utils/PagingHelper';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit {

  public users: any;
  public loading = true;
  public filter = {
    pageNumber: 1
  };
  public pagingInfo: any;

  constructor(private httpClient: HttpClient) { }

  ngOnInit() {
    this.reloadData();
  }

  private reloadData() {
    this.loading = true;
    this.httpClient.get(`/api/users/all?pageNumber=${this.filter.pageNumber}`).subscribe((response: any) => {
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
    this.reloadData();
  }
}
