import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PagingInfo, GetPagingInfo } from '../../utils/PagingHelper';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, TROGDOR_ROOM_OWNER } from '../services/auth.service';

@Component({
  selector: 'app-requests',
  templateUrl: './requests.component.html',
  styleUrls: ['./requests.component.scss'],
})
export class RequestsComponent implements OnInit {
  public loading = false;
  public pagingInfo: PagingInfo[];
  public showIgnoredTags: boolean;

  public isTrogdorRoomOwner = false;

  public filter = {
    tag: '',
    trackingStatusId: -1,
    trackingStatusName: '',
    pageNumber: 1,
    hasQuestions: 'true',
    sortBy: 'score'
  };

  model = { options: '2' };

  public questions: any[];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpClient: HttpClient,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filter.tag = params['tag'] || '';
      this.filter.trackingStatusId = +params['trackingStatusId'] || -1;
      this.filter.trackingStatusName = params['trackingStatusName'] || '';
      this.filter.hasQuestions = params['hasQuestions'] || 'any';
      this.filter.pageNumber = +params['pageNumber'] || 1;
      this.filter.sortBy = params['sortBy'] || 'score';
      this.reloadData();
    });

    this.authService.GetAuthDetails().subscribe(d => {
      this.isTrogdorRoomOwner = !!d.GetClaim(TROGDOR_ROOM_OWNER);
    });
  }

  public reloadData() {
    const query =
      `/api/MetaQuestions` +
      `?tag=${this.filter.tag}` +
      `&trackingStatusId=${this.filter.trackingStatusId}` +
      `&trackingStatusName=${this.filter.trackingStatusName}` +
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
          this.questions = response.data;
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
      tag.trackingStatusName = tracked ? 'Tracked' : 'Ignored';
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

      if (matchedTag && matchedTag.status !== 'Ignored') {
        return;
      }

      event.target.value = '';

      this.httpClient.post('/api/MetaQuestions/AddTag', {
        metaQuestionId: question.id,
        tagName: newTagName,
        requestType: 'Burninate'
      }).subscribe(_ => {
        if (matchedTag) {
          matchedTag.status = 'Tracked';
          matchedTag.statusId = 2;
        } else {
          question.mainTags = question.mainTags.concat([{
            questionCountOverTime: [],
            status: 'Tracked',
            statusId: 2,
            tagName: newTagName
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

  public setHasQuestions(newValue: boolean) {
    // this.filter.hasQuestions = newValue;
    console.log(newValue);
  }
}
