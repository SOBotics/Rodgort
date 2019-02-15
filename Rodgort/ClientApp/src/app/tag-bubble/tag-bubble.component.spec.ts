import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TagBubbleComponent } from './tag-bubble.component';

describe('TagBubbleComponent', () => {
  let component: TagBubbleComponent;
  let fixture: ComponentFixture<TagBubbleComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TagBubbleComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TagBubbleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
