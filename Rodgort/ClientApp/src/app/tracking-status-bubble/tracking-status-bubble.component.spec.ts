import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TrackingStatusBubbleComponent } from './tracking-status-bubble.component';

describe('TrackingStatusBubbleComponent', () => {
  let component: TrackingStatusBubbleComponent;
  let fixture: ComponentFixture<TrackingStatusBubbleComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TrackingStatusBubbleComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TrackingStatusBubbleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
