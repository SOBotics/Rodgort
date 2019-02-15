import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TagTrackingStatusAuditsComponent } from './tag-tracking-status-audits.component';

describe('TagTrackingStatusAuditsComponent', () => {
  let component: TagTrackingStatusAuditsComponent;
  let fixture: ComponentFixture<TagTrackingStatusAuditsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TagTrackingStatusAuditsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TagTrackingStatusAuditsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
