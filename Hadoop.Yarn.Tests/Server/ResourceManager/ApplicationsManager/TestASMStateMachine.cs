using Sharpen;

namespace Org.Apache.Hadoop.Yarn.Server.Resourcemanager.Applicationsmanager
{
	public class TestASMStateMachine
	{
		//  private static final Log LOG = LogFactory.getLog(TestASMStateMachine.class);
		//  private static RecordFactory recordFactory = RecordFactoryProvider.getRecordFactory(null);
		//  RMContext context = new RMContextImpl(new MemStore());
		//  EventHandler handler;
		//  private boolean snreceivedCleanUp = false;
		//  private boolean snAllocateReceived = false;
		//  private boolean launchCalled = false;
		//  private boolean addedApplication = false;
		//  private boolean removedApplication = false;
		//  private boolean launchCleanupCalled = false;
		//  private AtomicInteger waitForState = new AtomicInteger();
		//  private Configuration conf = new Configuration();
		//  @Before
		//  public void setUp() {
		//    context.getDispatcher().init(conf);
		//    context.getDispatcher().start();
		//    handler = context.getDispatcher().getEventHandler();
		//    new DummyAMLaunchEventHandler();
		//    new DummySNEventHandler();
		//    new ApplicationTracker();
		//    new MockAppplicationMasterInfo();
		//  }
		//
		//  @After
		//  public void tearDown() {
		//
		//  }
		//
		//  private class DummyAMLaunchEventHandler implements EventHandler<ASMEvent<AMLauncherEventType>> {
		//    AppAttempt application;
		//    AtomicInteger amsync = new AtomicInteger(0);
		//
		//    public DummyAMLaunchEventHandler() {
		//      context.getDispatcher().register(AMLauncherEventType.class, this);
		//    }
		//
		//    @Override
		//    public void handle(ASMEvent<AMLauncherEventType> event) {
		//      switch(event.getType()) {
		//      case LAUNCH:
		//        launchCalled = true;
		//        application = event.getApplication();
		//        context.getDispatcher().getEventHandler().handle(
		//            new ApplicationEvent(ApplicationEventType.LAUNCHED,
		//                application.getApplicationID()));
		//        break;
		//      case CLEANUP:
		//        launchCleanupCalled = true;
		//        break;
		//      }
		//    }
		//  }
		//
		//  private class DummySNEventHandler implements EventHandler<ASMEvent<SNEventType>> {
		//    AppAttempt application;
		//    AtomicInteger snsync = new AtomicInteger(0);
		//
		//    public DummySNEventHandler() {
		//      context.getDispatcher().register(SNEventType.class, this);
		//    }
		//
		//    @Override
		//    public void handle(ASMEvent<SNEventType> event) {
		//      switch(event.getType()) {
		//      case RELEASE:
		//        snreceivedCleanUp = true;
		//        break;
		//      case SCHEDULE:
		//        snAllocateReceived = true;
		//        application = event.getAppAttempt();
		//        context.getDispatcher().getEventHandler().handle(
		//            new AMAllocatedEvent(application.getApplicationID(),
		//                application.getMasterContainer()));
		//        break;
		//      }
		//    }
		//
		//  }
		//
		//  private class ApplicationTracker implements EventHandler<ASMEvent<ApplicationTrackerEventType>> {
		//    public ApplicationTracker() {
		//      context.getDispatcher().register(ApplicationTrackerEventType.class, this);
		//    }
		//
		//    @Override
		//    public void handle(ASMEvent<ApplicationTrackerEventType> event) {
		//      switch (event.getType()) {
		//      case ADD:
		//        addedApplication = true;
		//        break;
		//      case REMOVE:
		//        removedApplication = true;
		//        break;
		//      }
		//    }
		//  }
		//
		//  private class MockAppplicationMasterInfo implements
		//      EventHandler<ApplicationEvent> {
		//
		//    MockAppplicationMasterInfo() {
		//      context.getDispatcher().register(ApplicationEventType.class, this);
		//    }
		//    @Override
		//    public void handle(ApplicationEvent event) {
		//      LOG.info("The event type is " + event.getType());
		//    }
		//  }
		//
		//  private void waitForState( ApplicationState
		//      finalState, AppAttemptImpl masterInfo) throws Exception {
		//    int count = 0;
		//    while(masterInfo.getState() != finalState && count < 10) {
		//      Thread.sleep(500);
		//      count++;
		//    }
		//    Assert.assertEquals(finalState, masterInfo.getState());
		//  }
		//
		//  /* Test the state machine.
		//   *
		//   */
		//  @Test
		//  public void testStateMachine() throws Exception {
		//    ApplicationSubmissionContext submissioncontext = recordFactory.newRecordInstance(ApplicationSubmissionContext.class);
		//    submissioncontext.setApplicationId(recordFactory.newRecordInstance(ApplicationId.class));
		//    submissioncontext.getApplicationId().setId(1);
		//    submissioncontext.getApplicationId().setClusterTimestamp(System.currentTimeMillis());
		//
		//    AppAttemptImpl masterInfo = new AppAttemptImpl(context,
		//        conf, "dummyuser", submissioncontext, "dummyToken", StoreFactory
		//            .createVoidAppStore(), new AMLivelinessMonitor(context
		//            .getDispatcher().getEventHandler()));
		//
		//    context.getDispatcher().register(ApplicationEventType.class, masterInfo);
		//    handler.handle(new ApplicationEvent(
		//        ApplicationEventType.ALLOCATE, submissioncontext.getApplicationId()));
		//
		//    waitForState(ApplicationState.LAUNCHED, masterInfo);
		//    Assert.assertTrue(snAllocateReceived);
		//    Assert.assertTrue(launchCalled);
		//    Assert.assertTrue(addedApplication);
		//    handler
		//        .handle(new AMRegistrationEvent(masterInfo.getMaster()));
		//    waitForState(ApplicationState.RUNNING, masterInfo);
		//    Assert.assertEquals(ApplicationState.RUNNING, masterInfo.getState());
		//
		//    ApplicationStatus status = recordFactory
		//        .newRecordInstance(ApplicationStatus.class);
		//    status.setApplicationId(masterInfo.getApplicationID());
		//    handler.handle(new AMStatusUpdateEvent(status));
		//
		//    /* check if the state is still RUNNING */
		//
		//    Assert.assertEquals(ApplicationState.RUNNING, masterInfo.getState());
		//
		//    handler.handle(new AMFinishEvent(masterInfo.getApplicationID(),
		//        ApplicationState.COMPLETED, "", ""));
		//    waitForState(ApplicationState.COMPLETED, masterInfo);
		//    Assert.assertEquals(ApplicationState.COMPLETED, masterInfo.getState());
		//    /* check if clean up is called for everyone */
		//    Assert.assertTrue(launchCleanupCalled);
		//    Assert.assertTrue(snreceivedCleanUp);
		//    Assert.assertTrue(removedApplication);
		//
		//    /* check if expiry doesnt make it failed */
		//    handler.handle(new ApplicationEvent(ApplicationEventType.EXPIRE,
		//        masterInfo.getApplicationID()));
		//    Assert.assertEquals(ApplicationState.COMPLETED, masterInfo.getState());
		//  }
	}
}
