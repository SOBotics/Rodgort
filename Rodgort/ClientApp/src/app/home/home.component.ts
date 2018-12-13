import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PagingInfo, GetPagingInfo } from '../../utils/PagingHelper';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  public isAdmin = true;
  public pagingInfo: PagingInfo[];
  public filter = {
    tag: '',
    approvalStatus: -1,
    status: '',
    pageNumber: 1,
    sortBy: 'score'
  };

  public questions: any[];

  constructor(
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.reloadData();
  }

  public reloadData() {
    const query =
      `/api/MetaQuestions` +
      `?tag=${this.filter.tag}` +
      `&approvalStatus=${this.filter.approvalStatus}` +
      `&status=${this.filter.status}` +
      `&sortBy=${this.filter.sortBy}` +
      `&page=${this.filter.pageNumber}` +
      `&pageSize=10`;

    this.httpClient.get(query)
      .subscribe((response: any) => {
        if (response.totalPages > 0 && response.pageNumber > response.totalPages) {
          this.filter.pageNumber = 1;
          this.reloadData();
        } else {
          this.questions = response.data;
          this.pagingInfo = GetPagingInfo(response);
        }
      });
  }

  public setApprovalStatus(metaQuestionId: number, tagName: string, approved: boolean) {
    this.httpClient.post('/api/MetaQuestions/SetTagApprovalStatus', {
      metaQuestionId,
      tagName,
      approved
    }).subscribe(_ => {
      this.reloadData();
    });
  }

  public loadPage(pageNumber: number) {
    this.filter.pageNumber = pageNumber;
    this.reloadData();
  }
}
