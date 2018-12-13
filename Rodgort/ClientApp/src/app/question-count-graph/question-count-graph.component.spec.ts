import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { QuestionCountGraphComponent } from './question-count-graph.component';

describe('QuestionCountGraphComponent', () => {
  let component: QuestionCountGraphComponent;
  let fixture: ComponentFixture<QuestionCountGraphComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ QuestionCountGraphComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(QuestionCountGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
