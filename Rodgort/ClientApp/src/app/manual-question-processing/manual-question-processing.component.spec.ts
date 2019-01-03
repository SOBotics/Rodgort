import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ManualQuestionProcessingComponent } from './manual-question-processing.component';

describe('ManualQuestionProcessingComponent', () => {
  let component: ManualQuestionProcessingComponent;
  let fixture: ComponentFixture<ManualQuestionProcessingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ManualQuestionProcessingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ManualQuestionProcessingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
