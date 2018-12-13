import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PagingInfo, GetPagingInfo } from '../../utils/PagingHelper';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  public pagingInfo: PagingInfo[];
  public filter = {
    pageNumber: 1,
  };

  public questions: any[];

  constructor(
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.reloadData();
  }

  public reloadData() {
    this.httpClient.get(`/api/MetaQuestions?page=${this.filter.pageNumber}&pageSize=30`)
      .subscribe((response: any) => {
        console.log(response);
        if (response.totalPages > 0 && response.pageNumber > response.totalPages) {
          this.filter.pageNumber = 1;
          this.reloadData();
        } else {
          this.questions = response.data;
          this.pagingInfo = GetPagingInfo(response);
        }
      });
  }

  public loadPage(pageNumber: number) {
    this.filter.pageNumber = pageNumber;
    this.reloadData();
  }
}
