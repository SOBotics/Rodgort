import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PagingInfo, GetPagingInfo } from '../../utils/PagingHelper';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, TRIAGER } from '../services/auth.service';
import { tagTrackingStatus } from '../../constants/tag-tracking-status';
import * as moment from 'moment';

@Component({
  selector: 'app-requests',
  templateUrl: './requests.component.html',
  styleUrls: ['./requests.component.scss'],
})
export class RequestsComponent implements OnInit {
  public loading = false;
  public pagingInfo: PagingInfo[];
  public showIgnoredTags: boolean;

  public isRodgortTriager = false;

  public filter = {
    query: '',
    searchBy: '',
    trackingStatusId: -1,
    status: '',
    requestType: '',
    pageNumber: 1,
    hasQuestions: 'true',
    sortBy: 'age'
  };

  model = { options: '2' };

  tagTrackingStatus = tagTrackingStatus;

  public questions: any[];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpClient: HttpClient,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filter.query = params['query'] || '';
      this.filter.searchBy = params['searchBy'] || '';
      this.filter.trackingStatusId = +params['trackingStatusId'] || -1;
      this.filter.status = params['status'] || '';
      this.filter.requestType = params['requestType'] || '';
      this.filter.hasQuestions = params['hasQuestions'] || 'any';
      this.filter.pageNumber = +params['pageNumber'] || 1;
      this.filter.sortBy = params['sortBy'] || 'age';
      this.reloadData();
    });

    this.authService.GetAuthDetails().subscribe(d => {
      this.isRodgortTriager = d.HasRole(TRIAGER);
    });
  }

  public reloadData() {
    const query =
      `/api/MetaQuestions` +
      `?query=${this.filter.query}` +
      `&searchBy=${this.filter.searchBy}` +
      `&trackingStatusId=${this.filter.trackingStatusId}` +
      `&status=${this.filter.status}` +
      `&requestType=${this.filter.requestType}` +
      `&hasQuestions=${this.filter.hasQuestions}` +
      `&sortBy=${this.filter.sortBy}` +
      `&page=${this.filter.pageNumber}` +
      `&pageSize=10`;

    this.loading = true;
    this.httpClient.get(query)
      .subscribe((response: any) => {
        this.loading = false;
        if (response.totalPages > 0 && response.pageNumber > response.totalPages) {
          this.filter.pageNumber = 1;
          this.reloadData();
        } else {
          this.questions = response.data.map(d => ({
            ...d,
            creationDateLocal: moment.utc(d.creationDate).local().format('YYYY-MM-DD hh:mm:ss A')
          }));

          this.pagingInfo = GetPagingInfo(response);
        }
      });
  }

  public setTagTrackingStatus(metaQuestionId: number, tag: any, tracked: boolean) {
    this.httpClient.post('/api/MetaQuestions/SetTagTrackingStatus', {
      metaQuestionId,
      tagName: tag.tagName,
      tracked
    }).subscribe(_ => {
      tag.trackingStatusId = tracked ? 2 : 3;
      tag.trackingStatusName = tracked ? 'Tracked' : 'Ignored';
      if (!tracked) {
        tag.visibleIgnored = true;
      }
    });
  }

  public setRequestType(metaQuestionId: number, tag: any, requestType: string) {
    this.httpClient.post('/api/MetaQuestions/SetTagRequestType', {
      metaQuestionId,
      tagName: tag.tagName,
      requestType
    }).subscribe(_ => {
      tag.type = requestType;
    });
  }

  public onNewTagAdded(question: any, event: any) {
    if (event.key === 'Enter') {
      event.preventDefault();
      const newTagName = event.target.value;

      const matchedTag = question.mainTags.find((mt: any) => mt.tagName === newTagName);

      event.target.value = '';

      if (matchedTag && matchedTag.trackingStatusId !== tagTrackingStatus.IGNORED) {
        return;
      }

      this.httpClient.post('/api/MetaQuestions/AddTag', {
        metaQuestionId: question.id,
        tagName: newTagName
      }).subscribe(_ => {
        if (matchedTag) {
          matchedTag.trackingStatusName = 'Tracked';
          matchedTag.trackingStatusId = tagTrackingStatus.TRACKED;
        } else {
          question.mainTags = question.mainTags.concat([{
            questionCountOverTime: [],
            tagName: newTagName,
            trackingStatusId: tagTrackingStatus.TRACKED,
            trackingStatusName: 'Tracked'
          }]);
        }
      });
    }
  }

  public loadPage(pageNumber: number) {
    this.filter.pageNumber = pageNumber;
    this.applyFilter();
  }

  public applyFilter() {
    this.router.navigate(['/requests'], { queryParams: this.filter });
  }

  public showMore(tag: any) {
    tag.showingMore = true;
    if (tag.questionCountOverTime) {
      return;
    }

    this.httpClient.get(`/api/MetaQuestions/QuestionCountOverTime?tag=${tag.tagName}`)
      .subscribe((data: any) => {
        if (data.questionCountOverTime) {
          tag.questionCountOverTime = data.questionCountOverTime;
        } else {
          tag.questionCountOverTime = [];
        }
      });
  }

  public hideMore(tag: any) {
    tag.showingMore = false;
  }

  public setHasQuestions(newValue: boolean) {
    // this.filter.hasQuestions = newValue;
  }
}
